using UnityEngine;
using UnityEngine.UI;

public class ForkliftController : MonoBehaviour
{
    public float moveSpeed = 5f;         // 이동 속도
    public float rotationSpeed = 100f;  // 회전 속도
    public Transform fork;              // 포크 객체
    public float forkSpeed = 2f;        // 포크 이동 속도
    public float minForkHeight = 0.5f;  // 포크 최소 높이 (지면 기준)
    public float maxForkHeight = 2.5f;  // 포크 최대 높이 (지게차 내부 길이 기준)

    public RawImage screenImage;        // 화면에 표시할 RawImage
    public Texture defaultTexture;      // 기본 텍스처
    public Texture texture1;            // W 키를 눌렀을 때 표시할 텍스처
    public Texture texture2;            // S 키를 눌렀을 때 표시할 텍스처

    private Rigidbody rb;               // Rigidbody 변수
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody 컴포넌트 가져오기
        rb.freezeRotation = true;       // 회전은 스크립트로 처리

        if (screenImage != null && defaultTexture != null)
        {
            screenImage.texture = defaultTexture; // 초기 화면 설정
        }
    }

    void Update()
    {
        // 이동 및 회전 처리
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        MoveForklift();
        MoveFork();

        // 이미지 변경 처리
        ChangeScreenImage();
    }

    /// <summary>
    /// 지게차 이동 처리
    /// </summary>
    private void MoveForklift()
    {
        Vector3 movement = transform.forward * verticalInput * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));
    }

    /// <summary>
    /// 포크 조작 처리
    /// </summary>
    private void MoveFork()
    {
        if (fork != null)
        {
            Vector3 forkPosition = fork.localPosition;

            if (Input.GetKey(KeyCode.Q)) // 포크 올리기
            {
                forkPosition.y = Mathf.Clamp(forkPosition.y + forkSpeed * Time.deltaTime, minForkHeight, maxForkHeight);
            }
            else if (Input.GetKey(KeyCode.E)) // 포크 내리기
            {
                forkPosition.y = Mathf.Clamp(forkPosition.y - forkSpeed * Time.deltaTime, minForkHeight, maxForkHeight);
            }

            fork.localPosition = forkPosition; // 계산된 위치 적용
        }
    }

    /// <summary>
    /// W/S 키 입력에 따라 이미지 변경
    /// </summary>
    private void ChangeScreenImage()
    {
        if (screenImage != null)
        {
            if (Input.GetKey(KeyCode.W)) // W 키 입력 시
            {
                screenImage.texture = texture1;
            }
            else if (Input.GetKey(KeyCode.S)) // S 키 입력 시
            {
                screenImage.texture = texture2;
            }
            else // W/S 키 입력이 없을 때
            {
                screenImage.texture = defaultTexture;
            }
        }
    }
}
