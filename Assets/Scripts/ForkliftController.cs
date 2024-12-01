using UnityEngine;

public class ForkliftController : MonoBehaviour
{
    public float moveSpeed = 5f;         // 이동 속도
    public float rotationSpeed = 100f;   // 회전 속도
    public Transform fork;               // 포크 객체
    public float forkSpeed = 2f;         // 포크 이동 속도
    public float minForkHeight = 0.5f;   // 포크 최소 높이 (지면 기준)
    public float maxForkHeight = 2.5f;   // 포크 최대 높이 (지게차 내부 길이 기준)

    private Rigidbody rb;                // Rigidbody 변수
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody 컴포넌트 가져오기
        rb.freezeRotation = true; // 회전은 스크립트로 처리
    }

    void Update()
    {
        // 이동 입력 처리 (조이스틱 또는 키보드)
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // 물리적 이동 처리 (Rigidbody 사용)
        Vector3 movement = transform.forward * verticalInput * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // 회전 처리 (물리적 회전)
        float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));

        // 포크 조작
        if (Input.GetKey(KeyCode.Q)) // 포크 올리기
        {
            if (fork.localPosition.y < maxForkHeight) // 최대 높이 제한
            {
                fork.Translate(Vector3.up * forkSpeed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.E)) // 포크 내리기
        {
            if (fork.localPosition.y > minForkHeight) // 최소 높이 제한
            {
                fork.Translate(Vector3.down * forkSpeed * Time.deltaTime);
            }
        }
    }
}
