using UnityEngine;

public class CameraActivator : MonoBehaviour
{
    public Camera AGV_Camera; // AGV 메인 카메라

    void Start()
    {
        // 모든 카메라를 가져옵니다.
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera cam in cameras)
        {
            // MINIMAPCAMERA만 제외하고 비활성화
            if (cam.gameObject.name != "Minimap Camera")
            {
                cam.gameObject.SetActive(false);
            }
        }

        // AGV 카메라 활성화
        if (AGV_Camera != null)
        {
            AGV_Camera.gameObject.SetActive(true);
        }
    }
}
