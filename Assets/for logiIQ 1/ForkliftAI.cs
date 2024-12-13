using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForkliftAI : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridPartitioner gridPartitioner; // GridPartitioner로 변경
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float waypointThreshold = 0.5f;

    private Vector3 cellSize;
    private Vector2Int startCell;
    private Vector2Int targetCell;

    private List<Vector3> path;

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
        Debug.Log("ForkliftAI Start method called");

        if (!ValidateComponents())
        {
            Debug.LogError("Component validation failed!");
            enabled = false;
            return;
        }

        cellSize = gridPartitioner.CellSize;

        // 즉시 경로 찾기
        FindPathToTarget();
    }

    private bool ValidateComponents()
    {
        if (gridPartitioner == null)
        {
            Debug.LogError("GridPartitioner가 설정되지 않았습니다!");
            return false;
        }

        if (target == null)
        {
            Debug.LogError("Target이 설정되지 않았습니다!");
            return false;
        }

        return true;
    }

    private void FindPathToTarget()
    {
        startCell = WorldToGrid(transform.position);
        targetCell = WorldToGrid(target.position);

        Debug.Log($"Start Cell: {startCell}, Target Cell: {targetCell}");

        path = FindOptimizedPath(startCell, targetCell);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("유효한 경로를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"Path count: {path.Count}");
        StartCoroutine(MoveAlongPath());
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 gridOrigin = gridPartitioner.transform.position;
        Vector3 localPosition = worldPosition - gridOrigin;

        Vector2Int gridCell = new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize.x),
            Mathf.FloorToInt(localPosition.z / cellSize.z)
        );

        Debug.Log($"World Position: {worldPosition}");
        Debug.Log($"Grid Origin: {gridOrigin}");
        Debug.Log($"Cell Size: {cellSize}");
        Debug.Log($"Calculated Grid Cell: {gridCell}");

        return gridCell;
    }

    private List<Vector3> FindOptimizedPath(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();

        var startNode = new PathNode(start, 0, Heuristic(start, goal));
        openSet.Add(startNode);

        const int MAX_ITERATIONS = 1000;
        int iterations = 0;

        while (openSet.Count > 0 && iterations < MAX_ITERATIONS)
        {
            iterations++;

            var currentNode = openSet.OrderBy(n => n.FCost).First();

            if (currentNode.Position == goal)
            {
                return ReconstructPath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            foreach (var neighbor in GetValidNeighbors(currentNode.Position))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float newGCost = currentNode.GCost + 1;
                var existingNode = openSet.FirstOrDefault(n => n.Position == neighbor);

                if (existingNode == null)
                {
                    var newNode = new PathNode(
                        neighbor,
                        newGCost,
                        newGCost + Heuristic(neighbor, goal),
                        currentNode
                    );
                    openSet.Add(newNode);
                }
                else if (newGCost < existingNode.GCost)
                {
                    existingNode.GCost = newGCost;
                    existingNode.FCost = newGCost + Heuristic(neighbor, goal);
                    existingNode.Parent = currentNode;
                }
            }
        }

        Debug.LogWarning($"경로 탐색 실패 (반복 횟수: {iterations})");
        return null;
    }

    private List<Vector2Int> GetValidNeighbors(Vector2Int cell)
    {
        Vector2Int[] directions = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        return directions
            .Select(dir => cell + dir)
            .Where(IsValidAndUnobstructedCell)
            .ToList();
    }

    private bool IsValidAndUnobstructedCell(Vector2Int cell)
    {
        // GridPartitioner의 CanMoveToCell 메서드 사용
        return gridPartitioner.CanMoveToCell(cell.y, cell.x);
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
        if (path == null || path.Count == 0)
        {
            Debug.LogError("No path to move along!");
            yield break;
        }

        Debug.Log($"Starting to move along path with {path.Count} waypoints");

        foreach (Vector3 waypoint in path)
        {
            Debug.Log($"Moving to waypoint: {waypoint}");
            while (Vector3.Distance(transform.position, waypoint) > waypointThreshold)
            {
                // 더 직접적인 이동 방식
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    waypoint,
                    moveSpeed * Time.deltaTime
                );

                // 목표 방향으로 회전
                transform.LookAt(waypoint);

                yield return null;
            }
        }

        Debug.Log("지게차가 목표 지점에 도달했습니다.");
    }

    // 경로 시각화를 위한 디버그 기즈모
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}