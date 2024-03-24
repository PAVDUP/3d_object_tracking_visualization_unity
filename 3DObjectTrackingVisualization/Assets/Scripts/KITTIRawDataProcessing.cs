using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DataType;
using UnityEngine;
using Utils;

public class KITTIRawDataProcessing : MonoBehaviour
{
    private List<BoundingBox3D> _boundingBoxes;
    private CalibrationData _calibrationData;
    
    // 데이터 업데이트 주기를 정의 (예: 초 단위)
    public float updateInterval = 1.0f;
    private float _lastUpdateTime;

    private void Start()
    {
        InitializeDataFilesQueue();
    }

    public void Update()
    {
        if (Time.time - _lastUpdateTime > updateInterval && _dataFilesQueue.Count > 0)
        {
            _lastUpdateTime = Time.time;
            var nextDataFile = _dataFilesQueue.Dequeue();
            var dataLines = File.ReadAllLines(nextDataFile);
        
            // 예시: 파일명이 "calib"로 시작하면 캘리브레이션 데이터로 간주
            if (Path.GetFileName(nextDataFile).StartsWith("calib"))
            {
                _calibrationData = KITTIDataUtil.ParseCalibration(dataLines);
            }
            else // 그렇지 않으면 라벨 데이터로 간주
            {
                ProcessLabelData(dataLines);
                TransformBoundingBoxesToCameraView();
            }
        }
    }

    void ProcessLabelData(string[] labelLines)
    {
        _boundingBoxes.Clear(); // 기존 바운딩 박스를 지우고 새로 시작합니다.
        foreach (var line in labelLines)
        {
            if (line.StartsWith("DontCare")) continue; // "DontCare" 객체는 무시
            var parts = line.Split(' ');
            var boundingBox = new BoundingBox3D
            {
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
            _boundingBoxes.Add(boundingBox);
        }
    }
    
    public void TransformBoundingBoxesToCameraView()
    {
        var trMatrix = _calibrationData.TrVeloToCam * _calibrationData.RectificationMatrix;
        for (int i = 0; i < _boundingBoxes.Count; i++)
        {
            var box = _boundingBoxes[i];
            var transformedBox = KITTIDataUtil.TransformBoundingBox(box, trMatrix);
            _boundingBoxes[i] = transformedBox;
        }
    }

    #region Local Data 를 위한 Queue 초기화

    private Queue<string> _dataFilesQueue = new Queue<string>();

    public void InitializeDataFilesQueue()
    {
        var fileDirectory = Application.dataPath + "/path/to/your/data/files"; // Unity 프로젝트 내의 데이터 파일 경로
        var dataFiles = Directory.GetFiles(fileDirectory, "*.txt"); // 모든 .txt 파일을 가져옵니다.
        foreach (var file in dataFiles)
        {
            _dataFilesQueue.Enqueue(file);
        }
    }

    #endregion
}
