using UnityEngine;

public class ForkliftObstacleAvoidance : MonoBehaviour
{
    public AudioSource warningSound; // 경고음
    public Transform[] obstacles;    // YOLO에서 인식한 장애물 좌표
    public float detectionRadius = 5f; // 충돌 감지 반경

    void Update()
    {
        foreach (Transform obstacle in obstacles)
        {
            float distance = Vector3.Distance(transform.position, obstacle.position);
            if (distance < detectionRadius) // 충돌 위험 감지
            {
                if (!warningSound.isPlaying)
                {
                    warningSound.Play(); // 경고음 출력
                }
                AvoidObstacle(obstacle); // 경로 회피
            }
        }
    }

    void AvoidObstacle(Transform obstacle)
    {
        // 간단한 경로 회피 로직
        //transform.Translate(Vector3.right * Time.deltaTime * 2f);
    }
}
