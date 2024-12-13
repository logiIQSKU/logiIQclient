using Unity.Barracuda;
using UnityEngine;

public class YOLODetection : MonoBehaviour
{
    public NNModel modelAsset;  // ONNX 모델
    private IWorker worker;
    public RenderTexture renderTexture;  // 카메라가 렌더링한 텍스처
    private Texture2D inputTexture;

    void Start()
    {
        // Barracuda 모델 로딩
        worker = ModelLoader.Load(modelAsset).CreateWorker();
        inputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        // 카메라 렌더링 결과를 텍스처로 변환
        RenderTexture.active = renderTexture;
        inputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        inputTexture.Apply();

        // 텍스처를 Tensor로 변환
        Tensor inputTensor = new Tensor(inputTexture);

        // YOLO 모델 실행
        worker.Execute(inputTensor);
        Tensor outputTensor = worker.PeekOutput();

        // 결과 처리 (예: bounding box 추출)
        ProcessDetectionResults(outputTensor);
    }

    void ProcessDetectionResults(Tensor outputTensor)
    {
        // YOLO 모델의 outputTensor에서 객체 탐지 결과를 추출하고 처리하는 코드 작성
        // 예: bounding box, confidence score, 클래스 정보 등을 처리
    }

    void OnDestroy()
    {
        // 작업 종료시 worker 해제
        worker.Dispose();
    }
}
