using UnityEngine;

public class GridPartitioner : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GameObject plane; // 그리드를 표시할 평면
    [SerializeField] private int rows = 10; // 그리드 행 수
    [SerializeField] private int columns = 10; // 그리드 열 수
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어

    public Vector3 CellSize { get; private set; } // 각 셀의 크기
    private Vector3 planeSize; // 평면의 전체 크기
    private bool[,] cellAccess; // 셀 접근 가능 여부 배열

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
            Debug.LogError("평면 객체가 설정되지 않았습니다!");
            return false;
        }

        if (rows <= 0 || columns <= 0)
        {
            Debug.LogError("행 또는 열 수는 0보다 커야 합니다!");
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
            Debug.LogError("평면에 Renderer 컴포넌트가 없습니다!");
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
        Vector3 origin = plane.transform.position - planeSize / 2; // 평면의 왼쪽 하단 기준점
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // 셀의 중심 좌표 계산
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // 장애물 존재 여부 확인
                bool isObstacle = Physics.CheckBox(cellCenter, CellSize / 2, Quaternion.identity, obstacleLayer);

                // 접근 가능 여부 설정
                cellAccess[row, col] = !isObstacle;

                // 디버그 로그
                //Debug.Log($"셀[{row}, {col}] 중심: {cellCenter}, 장애물 여부: {isObstacle}");
            }
        }
    }

    public bool CanMoveToCell(int row, int col)
    {
        if (!IsWithinBounds(row, col))
        {
            Debug.LogWarning($"셀[{row}, {col}]은 그리드 범위를 벗어났습니다!");
            return false;
        }
        return cellAccess[row, col];
    }

    public bool IsWithinBounds(int row, int col)
    {
        // 행과 열이 그리드 범위 내에 있는지 확인
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }

    private void OnDrawGizmos()
    {
        if (plane == null || CellSize == Vector3.zero) return;

        Vector3 origin = plane.transform.position - planeSize / 2; // 평면의 왼쪽 하단 기준점

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // 셀 접근 가능 여부에 따라 색상 설정
                if (cellAccess != null && IsWithinBounds(row, col))
                {
                    Gizmos.color = cellAccess[row, col] ? Color.green : Color.red;
                }
                else
                {
                    Gizmos.color = Color.gray; // 범위를 벗어난 경우
                }

                Gizmos.DrawWireCube(cellCenter, new Vector3(CellSize.x, 0.01f, CellSize.z));
            }
        }
    }
}
