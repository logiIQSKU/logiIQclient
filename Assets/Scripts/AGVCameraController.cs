using UnityEngine;

public class AGVCameraController : MonoBehaviour
{
    public Transform agvTransform; // AGV 오브젝트의 Transform
    public Vector3 cameraOffset = new Vector3(0, 2.0f, 0); // AGV 위에 카메라 위치 조정
    public float rotationSpeed = 5.0f;

    void Start()
    {
        // AGV Transform 설정 (AGV에 카메라가 붙어 있는 경우 자동으로 설정)
        if (agvTransform == null)
        {
            agvTransform = transform.parent;
        }

        // 카메라 초기 위치 설정
        transform.position = agvTransform.position + cameraOffset;
    }

    void Update()
    {
        // AGV를 따라 카메라 위치 이동
        FollowAGV();

        // 마우스를 사용해 회전
        RotateCamera();
    }

    void FollowAGV()
    {
        Vector3 desiredPosition = agvTransform.position + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5.0f);
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // AGV 시점을 마우스로 회전 (Y축 회전만)
        agvTransform.Rotate(Vector3.up * mouseX);
        transform.Rotate(Vector3.right * -mouseY);
    }
}
