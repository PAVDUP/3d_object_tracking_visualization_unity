using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using DataType;
using SimpleJSON;

public class OnDeviceAIRawDataProcessor : RawDataProcessor
{
    private readonly string _rawDataFilePath = "Assets/_Project/RawData(Model Output Data Example)/ondeviceai";
    private readonly Queue<string> _dataQueue = new Queue<string>();
    
    // Variable for TCP communication
    public bool useLocal = true;
    private TcpListener _tcpListener;
    private Thread _tcpListenerThread;
    private TcpClient _connectedTcpClient;
    private const int Port = 8052; // 모의로 해둠.

    private void Start()
    {
        if (useLocal)
            InitializeDataFilesQueue();
        else
            InitializeTcpListener();
    }
    
    private void InitializeDataFilesQueue()
    {
        // 데이터 파일의 이름에서 숫자를 추출하여 정렬
        var rawDataFiles = Directory.GetFiles(_rawDataFilePath, "*.json");

        var orderedRawDataFiles = rawDataFiles.OrderBy(Path.GetFileNameWithoutExtension).ToList();

        // calib 파일과 label 파일을 짝지어 큐에 추가
        for (int i = 0; i < orderedRawDataFiles.Count; i++)
        {
            _dataQueue.Enqueue(orderedRawDataFiles[i]);
        }
        
        Debug.Log($"[KITTIRawDataProcessor] Data files queue initialized. Count: {_dataQueue.Count}");
    }
    
    private void InitializeTcpListener()
    {
        try
        {
            _tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingData));
            _tcpListenerThread.IsBackground = true;
            _tcpListenerThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing TCP listener: {e.Message}");
        }
    }

    private void ListenForIncomingData()
    {
        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        Debug.Log("Server is listening");

        while (true)
        {
            using (_connectedTcpClient = _tcpListener.AcceptTcpClient())
            {
                using (NetworkStream stream = _connectedTcpClient.GetStream())
                {
                    byte[] buffer = new byte[_connectedTcpClient.ReceiveBufferSize];

                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        _dataQueue.Enqueue(dataReceived);
                        Debug.Log($"Received data: {dataReceived}");
                    }
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    public void Update()
    {
        if (Time.time - LastUpdateTime > updateInterval && _dataQueue.Count > 0)
        {
            Debug.Log($"[OnDeviceAIRawDataProcessor] Processing data files..." + _dataQueue.Count);
            
            LastUpdateTime = Time.time;
            var rawDataFilePath = _dataQueue.Dequeue();

            // raw data 파일 처리
            var rawDataLines = File.ReadAllLines(rawDataFilePath);
            List<BoundingBox3D> boundingBox3Ds = ProcessRawData(rawDataLines);
            
            // 바운딩 박스 처리 완료 이벤트 발생
            string boundingBoxData = "";
            foreach (var box in boundingBox3Ds)
            {
                boundingBoxData += JsonUtility.ToJson(box);
            }
            // Debug.Log(boundingBoxData);
            onRawDataProcessed.Invoke(boundingBoxData);
            onBoundingBoxProcessed.Invoke(boundingBox3Ds, updateInterval);
        }
    }
    
    /// <summary>
    /// 한 번의 ProcessRawData 는 하나의 파일에 하나의 JsonArray를 처리한다고 가정
    /// </summary>
    /// <param name="rawDataLines"></param>
    public List<BoundingBox3D> ProcessRawData(string[] rawDataLines)
    {
        // JSON 데이터 예시를 문자열로 받아오는 곳을 가정
        JSONNode jsonData = GetJsonDataFromRawData(rawDataLines);
        List<BoundingBox3D> boundingBoxes = ProcessJsonData(jsonData);
        
        return boundingBoxes;
    }
    
    public JSONNode GetJsonDataFromRawData(string[] rawDataLines)
    {
        string overallData = "";
        
        foreach (var rawDataLine in rawDataLines)
        {
            overallData += rawDataLine;
        }

        JSONNode jsonNode = JSONNode.Parse(overallData);
        Debug.Log("[OnDeviceAIRawDataProcessor] GetJsonData Result : " + jsonNode.ToString());
        return jsonNode;
    }

    // For Testing
    
    public List<BoundingBox3D> ProcessJsonData(JSONNode jsonArrayData)
    {
        List<BoundingBox3D> boundingBoxes = new List<BoundingBox3D>();

        int count = 0;
        
        foreach (var jsonElementData in jsonArrayData)
        {
            var jsonData = jsonElementData.Value;
            
            List<float> boxes = new List<float>();
            foreach (var eachBox in jsonData["box"].AsArray)
            {
                boxes.Add(float.Parse(eachBox.Value.ToString()));
            }
            float distance = jsonData["distance"].AsFloat;
            if (distance== 0) continue;  // 실제 환경의 경우, 거리가 0 인 경우가 존재할 수 없음, 방지하기 위해 처리.
            distance /= 1000;  // mm -> m 단위로 변환
            
            float realWidth = Calculate3DScale(boxes, distance);
            float realHeight = realWidth;  // 높이와 너비를 같게 가정, 필요에 따라 변경 가능
            
            BoundingBox3D boundingBox = new BoundingBox3D
            {
                rawClassificationData = jsonData["cls_name"],
                classification = ClassifyObject(jsonData["cls_name"]),
                identifier = count++, // 임의의 식별자 - 현재 식별자가 OnDevice 에서 존재치 않음.
                center = CalculateCenter(distance, jsonData["angle"]),
                size = new Vector3(realWidth, realHeight, realWidth),  // 깊이도 같은 값으로 가정
                rotation = Quaternion.identity  // 회전 없음 - 현재는 회전 체크 불가함.
            };
            Debug.Log("[OnDeviceAIRawDataProcessor] ProcessJsonData Result : " + boundingBox);
            
            boundingBoxes.Add(boundingBox);
        }

        return boundingBoxes;
    }

    private float Calculate3DScale(List<float> box, float distance)
    {
        // 보이는 크기와 실제 거리를 사용하여 실제 크기 추정
        float apparentWidth = box[2] - box[0]; // 픽셀 단위.
        float fov = 120.0f; // FOV = 120 이라 전달 받음.
        float screenWidth = 1920.0f;  // 캡처 가로 해상도 (추정) => 바꿔야 함.
        float fovRad = fov * Mathf.Deg2Rad;  // 라디안
        float visibleWidthAtOneMeter = 2.0f * Mathf.Tan(fovRad / 2.0f);  // 1미터에서 카메라가 볼 수 있는 가로 거리
        float scalePerPixel = visibleWidthAtOneMeter / screenWidth;  // 픽셀 당 실제 거리

        // 실제 거리 계산 (미터)
        float realWidth = apparentWidth * scalePerPixel * (distance);
        return realWidth;
    }


    private Vector3 CalculateCenter(float distance, float angle)
    {
        // 각도를 라디안으로 변환하고, X와 Z 위치 계산
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(radian) * distance;
        float z = Mathf.Cos(radian) * distance;
        return new Vector3(x, 0, z);  // Y는 0으로 가정, 실제 사용에 따라 조정 필요
    }

    private BoundingBox3DType ClassifyObject(string className)
    {
        return className switch
        {
            "car" => BoundingBox3DType.Car,
            "pedestrian" => BoundingBox3DType.Pedestrian,
            _ => BoundingBox3DType.Misc
        };
    }
}
