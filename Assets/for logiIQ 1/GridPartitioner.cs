using UnityEngine;

public class GridPartitioner : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GameObject plane; // 직사각형 평면 오브젝트
    [SerializeField] private int rows = 10; // 행의 개수
    [SerializeField] private int columns = 10; // 열의 개수
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어

    public Vector3 CellSize { get; private set; } // 각 셀의 크기
    private Vector3 planeSize; // 평면의 실제 크기
    private bool[,] cellAccess; // 셀 접근 가능 여부

    private void Start()
    {
        if (!ValidateInputs()) return;

        CalculatePlaneSize();
        InitializeGrid();
        CheckObstacles();
    }

    private bool ValidateInputs()
    {
        if (plane == null)
        {
            Debug.LogError("평면 오브젝트를 설정하세요!");
            return false;
        }

        if (rows <= 0 || columns <= 0)
        {
            Debug.LogError("행과 열의 개수는 0보다 커야 합니다!");
            return false;
        }

        return true;
    }

    private void CalculatePlaneSize()
    {
        // 평면의 크기 계산
        Renderer planeRenderer = plane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("평면에 Renderer가 없습니다!");
            return;
        }

        planeSize = planeRenderer.bounds.size; // 평면의 실제 크기
        CellSize = new Vector3(planeSize.x / columns, 1, planeSize.z / rows); // 셀 크기 계산
        Debug.Log($"평면 크기: {planeSize}, 셀 크기: {CellSize}");
    }

    private void InitializeGrid()
    {
        // 셀 접근 가능 여부 배열 초기화
        cellAccess = new bool[rows, columns];
    }

    private void CheckObstacles()
    {
        Vector3 origin = plane.transform.position - planeSize / 2; // 평면의 왼쪽 아래 모서리
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // 셀 중심 계산
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // 장애물 확인
                bool isObstacle = Physics.CheckBox(cellCenter, CellSize / 2, Quaternion.identity, obstacleLayer);

                // 접근 가능 여부 저장
                cellAccess[row, col] = !isObstacle;

                // 디버그 로그
                Debug.Log($"셀[{row}, {col}] 중심: {cellCenter}, 장애물 감지 여부: {isObstacle}");
            }
        }
    }

    public bool CanMoveToCell(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            Debug.LogError("셀 인덱스가 범위를 벗어났습니다!");
            return false;
        }
        return cellAccess[row, col];
    }

    private void OnDrawGizmos()
    {
        // 평면 또는 셀 크기가 초기화되지 않은 경우 리턴
        if (plane == null || CellSize == Vector3.zero) return;

        Vector3 origin = plane.transform.position - planeSize / 2; // 평면의 왼쪽 아래 모서리
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // Gizmos로 셀 표시
                Gizmos.color = cellAccess != null && cellAccess[row, col] ? Color.green : Color.red;
                Gizmos.DrawWireCube(cellCenter, new Vector3(CellSize.x, 0.01f, CellSize.z));
            }
        }
    }
}
