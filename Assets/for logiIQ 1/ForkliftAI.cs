using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForkliftAI : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridPartitioner gridPartitioner;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float waypointThreshold = 0.5f;

    [Header("Load/Unload Positions")]
    [SerializeField] private Transform loadPosition;   // 적재 위치
    [SerializeField] private Transform unloadPosition; // 하역 위치
    [SerializeField] private GameObject cargo;         // 화물 오브젝트
    
    [Header("Emergency Stop Settings")]
    [SerializeField] private float detectionDistance = 2f; // 사람 감지 거리
    [SerializeField] private LayerMask personLayer;        // 사람 레이어
    [SerializeField] private AudioClip warningSound;       // 경고음

    private AudioSource audioSource;

    private Vector3 cellSize;
    private Vector2Int startCell;
    private Vector2Int loadCell;
    private Vector2Int unloadCell;

    private List<Vector3> path;
    private bool isLoaded = false; // 화물 적재 상태

    // PathNode 클래스 정의
    private class PathNode
    {
        public Vector2Int Position { get; }
        public float GCost { get; set; }
        public float FCost { get; set; }
        public PathNode Parent { get; set; }

        public PathNode(Vector2Int position, float gCost, float fCost, PathNode parent = null)
        {
            Position = position;
            GCost = gCost;
            FCost = fCost;
            Parent = parent;
        }
    }

    private void Start()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("ForkliftAI: Component validation failed!");
            enabled = false;
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>(); // 오디오 소스 추가
        cellSize = gridPartitioner.CellSize;

        loadCell = WorldToGrid(loadPosition.position);
        unloadCell = WorldToGrid(unloadPosition.position);

        FindPathToTarget(loadCell);
    }

    private bool ValidateComponents()
    {
        if (gridPartitioner == null || loadPosition == null || unloadPosition == null || cargo == null)
        {
            Debug.LogError("ForkliftAI: Required components are missing!");
            return false;
        }
        return true;
    }

    private void FindPathToTarget(Vector2Int targetCell)
    {
        startCell = WorldToGrid(transform.position);
        path = FindOptimizedPath(startCell, targetCell);

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"ForkliftAI: 경로를 찾을 수 없습니다. 시작 셀: {startCell}, 목표 셀: {targetCell}");
        }
        else
        {
            Debug.Log($"ForkliftAI: 경로 탐색 성공. 경로 길이: {path.Count}");
            StartCoroutine(MoveAlongPath());
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 gridOrigin = gridPartitioner.transform.position;
        Vector3 localPosition = worldPosition - gridOrigin;

        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize.x),
            Mathf.FloorToInt(localPosition.z / cellSize.z)
        );
    }

    private List<Vector3> FindOptimizedPath(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();

        openSet.Add(new PathNode(start, 0, Heuristic(start, goal)));

        while (openSet.Count > 0)
        {
            var currentNode = openSet.OrderBy(n => n.FCost).First();
            if (currentNode.Position == goal)
                return ReconstructPath(currentNode);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            foreach (var neighbor in GetValidNeighbors(currentNode.Position))
            {
                if (closedSet.Contains(neighbor)) continue;

                float newGCost = currentNode.GCost + 1;
                var existingNode = openSet.FirstOrDefault(n => n.Position == neighbor);

                if (existingNode == null)
                {
                    openSet.Add(new PathNode(neighbor, newGCost, newGCost + Heuristic(neighbor, goal), currentNode));
                }
                else if (newGCost < existingNode.GCost)
                {
                    existingNode.GCost = newGCost;
                    existingNode.FCost = newGCost + Heuristic(neighbor, goal);
                    existingNode.Parent = currentNode;
                }
            }
        }

        return null;
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int cell)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return directions
            .Select(dir => cell + dir)
            .Where(IsValidAndUnobstructedCell)
            .ToList();
    }

    private bool IsValidAndUnobstructedCell(Vector2Int cell)
    {
        Vector2Int unloadGridCell = WorldToGrid(unloadPosition.position);
        if (cell == unloadGridCell) return true; // 하역 위치 예외 처리

        // 사람 레이어를 무시하고 이동 가능 여부만 확인
        return gridPartitioner.IsWithinBounds(cell.y, cell.x) && gridPartitioner.CanMoveToCell(cell.y, cell.x);
    }


    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3> ReconstructPath(PathNode finalNode)
    {
        var path = new List<Vector3>();
        var current = finalNode;
        while (current != null)
        {
            path.Add(GetWorldPositionFromGridCell(current.Position));
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    private Vector3 GetWorldPositionFromGridCell(Vector2Int cell)
    {
        Vector3 gridOrigin = gridPartitioner.transform.position;
        return new Vector3(
            cell.x * cellSize.x + cellSize.x / 2 + gridOrigin.x,
            transform.position.y,
            cell.y * cellSize.z + cellSize.z / 2 + gridOrigin.z
        );
    }

    private IEnumerator MoveAlongPath()
    {
        foreach (Vector3 waypoint in path)
        {
            while (Vector3.Distance(transform.position, waypoint) > waypointThreshold)
            {
                // 사람 감지 시 긴급 정지
                if (CheckForPerson())
                {
                    yield return StartCoroutine(HandleEmergencyStop());
                }

                // 정상 이동
                transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
                transform.LookAt(waypoint);
                yield return null;
            }
            Debug.Log("웨이포인트 도착 완료.");
        }

        // 적재 및 하역 절차
        if (!isLoaded)
        {
            LoadCargo();
            FindPathToTarget(WorldToGrid(unloadPosition.position));
        }
        else
        {
            StartCoroutine(UnloadAndReturnSequence());
        }
    }

    private void LoadCargo()
    {
        cargo.SetActive(true);
        cargo.transform.SetParent(transform);
        cargo.transform.localPosition = new Vector3(0, 0.11f, 0.8f);
        isLoaded = true;
        Debug.Log("화물 적재 완료.");
    }

    private IEnumerator UnloadAndReturnSequence()
    {
        UnloadCargo();

        // 뒤로 2미터 이동
        yield return MoveToPosition(transform.position - transform.forward * 2.0f);

        // 출발 위치로 돌아가기
        ReturnToStartPosition();
    }

    private void UnloadCargo()
    {
        cargo.transform.SetParent(null);

        // 원하는 하역 위치를 직접 설정
        Vector3 targetPosition = new Vector3(47.4210014f, 0.0130000003f, 42.9119987f);
        cargo.transform.position = targetPosition;

        cargo.transform.rotation = Quaternion.identity; // 회전 초기화
        isLoaded = false;

        Debug.Log($"화물 하역 완료: 위치 = {cargo.transform.position}");
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void ReturnToStartPosition()
    {
        FindPathToTarget(WorldToGrid(loadPosition.position));
    }
    
    private bool CheckForPerson()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; // 지게차의 위에서 Ray 발사

        // BoxCast를 통해 충돌 감지 (더 넓은 범위로 감지)
        if (Physics.BoxCast(rayStart, Vector3.one * 0.5f, transform.forward, out hit, Quaternion.identity, detectionDistance, personLayer))
        {
            Debug.Log("긴급 정지: 사람 감지됨 - " + hit.collider.name);
            return true;
        }

        return false;
    }

    private IEnumerator HandleEmergencyStop()
    {
        Debug.Log("긴급 정지! 경고음 재생 중...");
        moveSpeed = 0f; // 이동 정지

        float elapsedTime = 0f; // 경과 시간 추적
        float warningDuration = 5f; // 총 경고 시간 (5초)

        while (elapsedTime < warningDuration)
        {
            audioSource.PlayOneShot(warningSound); // 경고음 재생
            yield return new WaitForSeconds(warningSound.length); // 경고음 길이만큼 대기
            elapsedTime += warningSound.length; // 경과 시간 추가
        }

        RemovePersonInPath(); // 사람 오브젝트 제거
        moveSpeed = 5.0f; // 이동 속도 복구
        Debug.Log("긴급 정지 해제. 이동 재개.");
    }


    private void RemovePersonInPath()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        if (Physics.BoxCast(rayStart, Vector3.one * 0.5f, transform.forward, out hit, Quaternion.identity, detectionDistance, personLayer))
        {
            Debug.Log("사람 제거됨: " + hit.collider.gameObject.name);
            Destroy(hit.collider.gameObject); // 감지된 사람 오브젝트 삭제
        }
    }

}