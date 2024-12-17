using UnityEngine;
using Unity.Barracuda;

public class YOLOInference : MonoBehaviour
{
    public NNModel modelAsset;      // ONNX 모델 연결
    public Texture2D inputImage;    // 테스트 이미지 연결

    private Model runtimeModel;
    private IWorker worker;

    void Start()
    {
        // 모델 로드 및 Worker 생성
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);

        // 입력 데이터 준비
        Tensor inputTensor = PrepareInput(inputImage);

        // 추론 실행
        worker.Execute(inputTensor);

        // 출력 가져오기
        Tensor outputTensor = worker.PeekOutput();
        ProcessOutput(outputTensor);

        // Tensor 정리
        inputTensor.Dispose();
        outputTensor.Dispose();
    }

    Tensor PrepareInput(Texture2D image)
    {
        // 입력 이미지 크기 조정 (640x640)
        Texture2D resizedImage = ResizeImage(image, 640, 640);

        // Tensor 생성 (크기: [1, 3, 640, 640])
        Tensor inputTensor = new Tensor(1, 3, 640, 640);

        // 이미지 데이터를 텐서에 채우기
        for (int y = 0; y < 640; y++)
        {
            for (int x = 0; x < 640; x++)
            {
                Color pixel = resizedImage.GetPixel(x, y);
                inputTensor[0, 0, y, x] = pixel.r; // Red 채널
                inputTensor[0, 1, y, x] = pixel.g; // Green 채널
                inputTensor[0, 2, y, x] = pixel.b; // Blue 채널
            }
        }

        return inputTensor;
    }

    Texture2D ResizeImage(Texture2D source, int width, int height)
    {
        // 이미지 크기 조정
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    void ProcessOutput(Tensor output)
    {
        Debug.Log($"Output Tensor Shape: {output.shape}");

        int batch = output.shape[0];
        int heads = output.shape[1];
        int gridWidth = output.shape[2];
        int channels = output.shape[3];

        for (int head = 0; head < heads; head++)
        {
            for (int grid = 0; grid < gridWidth; grid++)
            {
                // Confidence 값 출력
                float confidence = output[0, head, grid, 4];
                Debug.Log($"Grid {grid}, Head {head}, Confidence: {confidence}");

                if (confidence > 0.5f) // Confidence 조건 추가
                {
                    float x = output[0, head, grid, 0];
                    float y = output[0, head, grid, 1];
                    float w = output[0, head, grid, 2];
                    float h = output[0, head, grid, 3];

                    Debug.Log($"Detected Object: Head={head}, Grid={grid}, Confidence={confidence}, Box: [{x}, {y}, {w}, {h}]");
                }
            }
        }
    }


    void OnDestroy()
    {
        // Worker 정리
        if (worker != null)
        {
            worker.Dispose();
        }
    }
}
