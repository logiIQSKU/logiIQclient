using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform externalPosition; // �ܺ� ī�޶� ��ġ
    public Transform internalPosition; // ���� ī�޶� ��ġ
    public float transitionSpeed = 2.0f; // ī�޶� �̵� �ӵ�
    public AudioClip seatbeltSound; // ������Ʈ ���� �Ҹ�
    public AudioSource audioSource; // �Ҹ� ����� AudioSource
    public GameObject warningImage; // ��� �̹��� (Optional)

    private bool isInVehicle = false; // ����/�ܺ� ���� üũ
    private bool isSeatbeltFastened = false; // ������Ʈ ���� ���� üũ

    void Update()
    {
        if (!isInVehicle && Input.GetKeyDown(KeyCode.Return)) // Enter Ű�� ���� ����
        {
            isInVehicle = true;
            StartCoroutine(MoveCameraAndUpdateUI());
        }
        else if (isInVehicle && !isSeatbeltFastened && Input.GetKeyDown(KeyCode.Return)) // Enter Ű�� ������Ʈ ����
        {
            PlaySeatbeltSound();
        }
    }

    private System.Collections.IEnumerator MoveCameraAndUpdateUI()
    {
        // ī�޶� �̵�
        Transform targetPosition = internalPosition;
        while (Vector3.Distance(transform.position, targetPosition.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        // ī�޶� �̵� �Ϸ� �� ��ġ ����
        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;

        // ���� ����
        isInVehicle = true;

        // UI ������Ʈ (��� �̹��� ǥ��, �ʿ��)
        if (warningImage != null)
        {
            warningImage.SetActive(true); // ��� �̹��� ǥ�� (Optional)
        }
    }

    private void PlaySeatbeltSound()
    {
        // ������Ʈ ���� �Ҹ� ���
        if (audioSource != null && seatbeltSound != null)
        {
            audioSource.PlayOneShot(seatbeltSound);
        }

        // ������Ʈ ���� ���� ����
        isSeatbeltFastened = true;

        // UI ������Ʈ (��� �̹��� �����)
        if (warningImage != null)
        {
            warningImage.SetActive(false);
        }

        // ��ũ��Ʈ ��Ȱ��ȭ
        this.enabled = false;
    }
}
