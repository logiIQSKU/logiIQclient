using UnityEngine;

public class MinimapController : MonoBehaviour
{
    // 자율주행차(AGV)와 미니맵에 표시할 빨간 점 아이콘을 참조
    public Transform logiIQ_AGV; // 자율주행차 오브젝트
    public RectTransform minimapAgvIcon; // 미니맵에 표시할 빨간 점 (UI Image)

    private Vector3 minimapCenter; // 미니맵의 중심 위치
    private float minimapSize = 100f; // 미니맵 크기 (조정 가능)

    void Start()
    {
        // 미니맵의 중심 위치 설정 (미니맵의 RectTransform 기준으로)
        minimapCenter = minimapAgvIcon.position;
    }

    void Update()
    {
        // 자율주행차의 위치 추적 (X, Z 좌표만 사용)
        Vector3 agvPosition = logiIQ_AGV.position;

        // 자율주행차의 X, Z 값을 미니맵 크기에 맞게 변환 (단위 변환)
        Vector2 minimapPosition = new Vector2(agvPosition.x, agvPosition.z);

        // 미니맵 크기 (minimapSize)와 비율 맞추기
        minimapPosition = (minimapPosition / minimapSize) * 100f;  // 적당한 비율로 조정

        // 미니맵의 중심을 Vector2로 변환하여 더하기
        minimapAgvIcon.anchoredPosition = new Vector2(minimapCenter.x, minimapCenter.z) + minimapPosition;

        // 위치와 아이콘 확인을 위한 디버깅 로그
        Debug.Log("AGV Position: " + agvPosition);
        Debug.Log("Minimap Position: " + minimapPosition);
    }

}
