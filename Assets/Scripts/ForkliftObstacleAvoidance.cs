using UnityEngine;
using UnityEngine.UI;

public class ForkliftObstacleAvoidance : MonoBehaviour
{
    public AudioSource warningSound;    // 경고음
    public Transform[] obstacles;       // YOLO에서 인식한 장애물 좌표
    public float detectionRadius = 5f;  // 충돌 감지 반경
    public RawImage warningImage;       // 경고 이미지 (RawImage)

    private bool isObstacleDetected = false; // 장애물 감지 상태

    void Start()
    {
        // 초기화: 경고 이미지를 비활성화
        if (warningImage != null)
        {
            warningImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        isObstacleDetected = false;

        foreach (Transform obstacle in obstacles)
        {
            float distance = Vector3.Distance(transform.position, obstacle.position);
            if (distance < detectionRadius)
            {
                isObstacleDetected = true;

                if (!warningSound.isPlaying)
                {
                    warningSound.Play();
                }
            }
        }

        // 장애물 감지 상태에 따라 경고 이미지 활성화/비활성화
        if (warningImage != null)
        {
            warningImage.gameObject.SetActive(isObstacleDetected);
        }
    }

    void AvoidObstacle(Transform obstacle)
    {
        // 간단한 경로 회피 로직 (필요 시 구현)
        // transform.Translate(Vector3.right * Time.deltaTime * 2f);
    }
}
