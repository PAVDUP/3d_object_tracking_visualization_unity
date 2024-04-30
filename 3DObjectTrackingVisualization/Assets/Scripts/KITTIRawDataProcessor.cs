using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataType;
using UnityEngine;
using Utils;

public class KITTIRawDataProcessor : RawDataProcessor
{
    private CalibrationData _calibrationData;

    // 데이터 파일 경로
    private string _calibFilesPath = "Assets/RawData(Model Output Data Example)/calib";
    private string _labelFilesPath = "Assets/RawData(Model Output Data Example)/label_2";

    private Queue<(string calibFilePath, string labelFilePath)> _dataFilesQueue = new Queue<(string, string)>();

    private void Start()
    {
        InitializeDataFilesQueue();
    }

    private void InitializeDataFilesQueue()
    {
        // 데이터 파일의 이름에서 숫자를 추출하여 정렬
        var calibFiles = Directory.GetFiles(_calibFilesPath, "*.txt");
        var labelFiles = Directory.GetFiles(_labelFilesPath, "*.txt");

        var orderedCalibFiles = calibFiles.OrderBy(Path.GetFileNameWithoutExtension).ToList();
        var orderedLabelFiles = labelFiles.OrderBy(Path.GetFileNameWithoutExtension).ToList();

        // calib 파일과 label 파일을 짝지어 큐에 추가
        for (int i = 0; i < orderedCalibFiles.Count && i < orderedLabelFiles.Count; i++)
        {
            _dataFilesQueue.Enqueue((orderedCalibFiles[i], orderedLabelFiles[i]));
        }
        
        Debug.Log($"[KITTIRawDataProcessor] Data files queue initialized. Count: {_dataFilesQueue.Count}");
    }

    public void Update()
    {
        if (Time.time - LastUpdateTime > updateInterval && _dataFilesQueue.Count > 0)
        {
            Debug.Log($"[KITTIRawDataProcessor] Processing data files...");
            
            LastUpdateTime = Time.time;
            var (calibFilePath, labelFilePath) = _dataFilesQueue.Dequeue();

            // calib 파일 처리
            var calibDataLines = File.ReadAllLines(calibFilePath);
            _calibrationData = KITTIDataUtil.ParseCalibration(calibDataLines);
            
            // label 파일 처리
            var labelDataLines = File.ReadAllLines(labelFilePath);
            List<BoundingBox3D> boundingBox3Ds = ProcessLabelData(labelDataLines);
            
            // 바운딩 박스를 카메라 뷰로 변환
            boundingBox3Ds = TransformBoundingBoxesToCameraView(boundingBox3Ds);
            
            // 바운딩 박스 처리 이벤트 호출
            Debug.Log($"[KITTIRawDataProcessor] Bounding boxes processed. Count: {boundingBox3Ds.Count}");

            string boundingBoxData = "";
            foreach (var box in boundingBox3Ds)
            {
                boundingBoxData += $"{{Classification: {box.Classification}, Center: {box.Center}, Size: {box.Size}, Rotation: {box.Rotation}}}\n";
            }
            Debug.Log(boundingBoxData);
            
            onBoundingBoxProcessed.Invoke(boundingBox3Ds);
        }
    }

    List<BoundingBox3D> ProcessLabelData(string[] labelLines)
    {
        List<BoundingBox3D> boundingBoxes = new List<BoundingBox3D>();
        foreach (var line in labelLines)
        {
            if (line.StartsWith("DontCare")) continue; // "DontCare" 객체는 무시
            var parts = line.Split(' ');
            
            var classification = parts[0]; // 첫 번째 요소가 Classification 정보
            var boundingBox = new BoundingBox3D
            {
                Classification = classification,
                Center = new Vector3(
                    float.Parse(parts[11], CultureInfo.InvariantCulture),
                    float.Parse(parts[12], CultureInfo.InvariantCulture),
                    float.Parse(parts[13], CultureInfo.InvariantCulture)),
                Size = new Vector3(
                    float.Parse(parts[8], CultureInfo.InvariantCulture),
                    float.Parse(parts[9], CultureInfo.InvariantCulture),
                    float.Parse(parts[10], CultureInfo.InvariantCulture)),
                Rotation = KITTIDataUtil.RotationFromYaw(float.Parse(parts[14], CultureInfo.InvariantCulture))
            };
            boundingBoxes.Add(boundingBox);
        }

        return boundingBoxes;
    }
    
    public List<BoundingBox3D> TransformBoundingBoxesToCameraView(List<BoundingBox3D> boundingBox3Ds)
    {
        List<BoundingBox3D> transformedBoxes = boundingBox3Ds;
        var trMatrix = _calibrationData.TrVeloToCam * _calibrationData.RectificationMatrix;
        for (int i = 0; i < transformedBoxes.Count; i++)
        {
            var box = transformedBoxes[i];
            var transformedBox = KITTIDataUtil.TransformBoundingBox(box, trMatrix);
            transformedBox.Center =
                new Vector3(transformedBox.Center.z, -transformedBox.Center.x, -transformedBox.Center.y);
            transformedBoxes[i] = transformedBox;
        }

        return boundingBox3Ds;
    }
}
