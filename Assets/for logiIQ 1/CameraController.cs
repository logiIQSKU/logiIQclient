using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform externalPosition; // 외부 카메라 위치
    public Transform internalPosition; // 내부 카메라 위치
    public float transitionSpeed = 2.0f; // 카메라 이동 속도
    public AudioClip seatbeltSound; // 안전벨트 착용 소리
    public AudioSource audioSource; // 소리 재생용 AudioSource
    public GameObject warningImage; // 경고 이미지 (Optional)

    private bool isInVehicle = false; // 내부/외부 상태 체크
    private bool isSeatbeltFastened = false; // 안전벨트 착용 상태 체크

    void Update()
    {
        if (!isInVehicle && Input.GetKeyDown(KeyCode.Return)) // Enter 키로 차량 승차
        {
            isInVehicle = true;
            StartCoroutine(MoveCameraAndUpdateUI());
        }
        else if (isInVehicle && !isSeatbeltFastened && Input.GetKeyDown(KeyCode.Return)) // Enter 키로 안전벨트 착용
        {
            PlaySeatbeltSound();
        }
    }

    private System.Collections.IEnumerator MoveCameraAndUpdateUI()
    {
        // 카메라 이동
        Transform targetPosition = internalPosition;
        while (Vector3.Distance(transform.position, targetPosition.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        // 카메라 이동 완료 후 위치 고정
        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;

        // 상태 변경
        isInVehicle = true;

        // UI 업데이트 (경고 이미지 표시, 필요시)
        if (warningImage != null)
        {
            warningImage.SetActive(true); // 경고 이미지 표시 (Optional)
        }
    }

    private void PlaySeatbeltSound()
    {
        // 안전벨트 착용 소리 재생
        if (audioSource != null && seatbeltSound != null)
        {
            audioSource.PlayOneShot(seatbeltSound);
        }

        // 안전벨트 착용 상태 갱신
        isSeatbeltFastened = true;

        // UI 업데이트 (경고 이미지 숨기기)
        if (warningImage != null)
        {
            warningImage.SetActive(false);
        }

        // 스크립트 비활성화
        this.enabled = false;
    }
}
