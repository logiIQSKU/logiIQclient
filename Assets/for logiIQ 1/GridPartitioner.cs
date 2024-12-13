using UnityEngine;

public class GridPartitioner : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GameObject plane; // ���簢�� ��� ������Ʈ
    [SerializeField] private int rows = 10; // ���� ����
    [SerializeField] private int columns = 10; // ���� ����
    [SerializeField] private LayerMask obstacleLayer; // ��ֹ� ���̾�

    public Vector3 CellSize { get; private set; } // �� ���� ũ��
    private Vector3 planeSize; // ����� ���� ũ��
    private bool[,] cellAccess; // �� ���� ���� ����

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
            Debug.LogError("��� ������Ʈ�� �����ϼ���!");
            return false;
        }

        if (rows <= 0 || columns <= 0)
        {
            Debug.LogError("��� ���� ������ 0���� Ŀ�� �մϴ�!");
            return false;
        }

        return true;
    }

    private void CalculatePlaneSize()
    {
        // ����� ũ�� ���
        Renderer planeRenderer = plane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("��鿡 Renderer�� �����ϴ�!");
            return;
        }

        planeSize = planeRenderer.bounds.size; // ����� ���� ũ��
        CellSize = new Vector3(planeSize.x / columns, 1, planeSize.z / rows); // �� ũ�� ���
        Debug.Log($"��� ũ��: {planeSize}, �� ũ��: {CellSize}");
    }

    private void InitializeGrid()
    {
        // �� ���� ���� ���� �迭 �ʱ�ȭ
        cellAccess = new bool[rows, columns];
    }

    private void CheckObstacles()
    {
        Vector3 origin = plane.transform.position - planeSize / 2; // ����� ���� �Ʒ� �𼭸�
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // �� �߽� ���
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // ��ֹ� Ȯ��
                bool isObstacle = Physics.CheckBox(cellCenter, CellSize / 2, Quaternion.identity, obstacleLayer);

                // ���� ���� ���� ����
                cellAccess[row, col] = !isObstacle;

                // ����� �α�
                Debug.Log($"��[{row}, {col}] �߽�: {cellCenter}, ��ֹ� ���� ����: {isObstacle}");
            }
        }
    }

    public bool CanMoveToCell(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            Debug.LogError("�� �ε����� ������ ������ϴ�!");
            return false;
        }
        return cellAccess[row, col];
    }

    private void OnDrawGizmos()
    {
        // ��� �Ǵ� �� ũ�Ⱑ �ʱ�ȭ���� ���� ��� ����
        if (plane == null || CellSize == Vector3.zero) return;

        Vector3 origin = plane.transform.position - planeSize / 2; // ����� ���� �Ʒ� �𼭸�
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 cellCenter = origin + new Vector3(
                    col * CellSize.x + CellSize.x / 2,
                    0,
                    row * CellSize.z + CellSize.z / 2
                );

                // Gizmos�� �� ǥ��
                Gizmos.color = cellAccess != null && cellAccess[row, col] ? Color.green : Color.red;
                Gizmos.DrawWireCube(cellCenter, new Vector3(CellSize.x, 0.01f, CellSize.z));
            }
        }
    }
}
