using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
    public AudioSource audioSource; // AudioSource 컴포넌트 연결
    public string targetTag = "Obstacle"; // 충돌 대상의 태그

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is missing!");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 충돌한 물체가 특정 태그를 가진 경우에만 소리 재생
        if (collision.gameObject.CompareTag(targetTag))
        {
            audioSource.Play();
        }
    }
}
