using System.Collections.Generic;
using DataType;
using DG.Tweening;
using ScriptableObjectV;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VisualizeModule
{
    public class BoundingBox3DVisualizer : MonoBehaviour
    {
        public ClassificationPrefabs classificationPrefabsSetting;
        
        public Material boundingBoxMaterial;
        public Transform frontCameraTransform; // 카메라의 Transform 참조!!!!!!!!!!!
        public Transform backCameraTransform; 

        // 현재 활성화된 바운딩 박스 추적
        private readonly List<BoundingBox3DHolder> _currentBoundingBoxObjects = new List<BoundingBox3DHolder>();
        
        // Events
        public UnityEvent<string, string> onNewBoundingBoxesVisualized;
        
        private void Start()
        {
            if (frontCameraTransform == null || backCameraTransform == null)
            {
                Debug.LogError("[BoundingBox3DVisualizer] One or both camera transforms are not set. Please set the camera transforms.");
            }
            
            if (classificationPrefabsSetting == null)
            {
                Debug.LogError("[BoundingBox3DVisualizer] classificationPrefabsSetting is not set. Please set the classification prefabs setting.");
            }
        }

        /// <summary>
        /// 주어진 바운딩 박스 데이터로 바운딩 박스를 시각화
        /// </summary>
        /// <param name="boundingBoxes">시각화할 바운딩 박스 데이터 리스트</param>
        public void VisualizeBoundingBoxes(List<BoundingBox3D> boundingBoxes, float updateInterval)
        {
            List<BoundingBox3DHolder> notRemovedBoundingBoxes = new List<BoundingBox3DHolder>();
            
            for (int i = boundingBoxes.Count - 1; i >= 0; i--) 
            {
                var bbox = boundingBoxes[i];
                Transform selectedCameraTransform = bbox.cameraType == BoundingBoxCameraType.Front ? frontCameraTransform : backCameraTransform;
                
                foreach (var currentBoundingBox in _currentBoundingBoxObjects)
                {
                    if (bbox.identifier == currentBoundingBox.BoundingBox3D.identifier)
                    {
                        Vector3 worldPosition = selectedCameraTransform.TransformPoint(bbox.center);
                        Quaternion worldRotation = selectedCameraTransform.rotation * bbox.rotation;

                        // 칼만 필터를 통해 상태 업데이트
                        currentBoundingBox.UpdateState(worldPosition, worldRotation);

                        currentBoundingBox.transform.DOMove(worldPosition, updateInterval);
                        currentBoundingBox.transform.DORotate(worldRotation.eulerAngles, updateInterval);

                        notRemovedBoundingBoxes.Add(currentBoundingBox);
                        _currentBoundingBoxObjects.Remove(currentBoundingBox);
                        boundingBoxes.Remove(bbox);
                        break;
                    }
                }
            }

            ClearBoundingBoxes(); 

            foreach (var bbox in boundingBoxes)
            {
                Transform selectedCameraTransform = bbox.cameraType == BoundingBoxCameraType.Front ? frontCameraTransform : backCameraTransform;
                
                Vector3 worldPosition = selectedCameraTransform.TransformPoint(bbox.center);
                Quaternion worldRotation = selectedCameraTransform.rotation * bbox.rotation;

                GameObject bboxObject = InstantiatePrefab(bbox);
                bboxObject.transform.position = worldPosition;
                bboxObject.transform.localScale = bbox.size;
                bboxObject.transform.rotation = worldRotation;

                string jsonBboxInfo = JsonUtility.ToJson(bbox);
                Debug.Log($"[BoundingBox3DVisualizer] New bounding box visualized. {jsonBboxInfo}");
                onNewBoundingBoxesVisualized?.Invoke("New Object Detected", jsonBboxInfo);

                BoundingBox3DHolder newHolder = bboxObject.AddComponent<BoundingBox3DHolder>();
                newHolder.SetBoundingBox3DInfo(bbox);
                newHolder.UpdateState(worldPosition, worldRotation);  // 초기 칼만 필터 상태 설정

                _currentBoundingBoxObjects.Add(newHolder);
            }

            _currentBoundingBoxObjects.AddRange(notRemovedBoundingBoxes);
        }
        
        private GameObject InstantiatePrefab(BoundingBox3D bbox)
        {
            GameObject bboxObject = null;

            foreach (var prefab in classificationPrefabsSetting.classificationPrefabs)
            {
                if (bbox.classification == prefab.classificationType)
                {
                    bboxObject = Instantiate(prefab.classificationPrefabCandidates[0]);
                    break;
                }
            }

            if (bboxObject == null)
            {
                bboxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (boundingBoxMaterial != null)
                    bboxObject.GetComponent<Renderer>().material = boundingBoxMaterial;
            }

            return bboxObject;
        }

        /// <summary>
        /// 모든 바운딩 박스 오브젝트를 제거
        /// </summary>
        private void ClearBoundingBoxes()
        {
            foreach (var obj in _currentBoundingBoxObjects)
            {
                Destroy(obj.gameObject);
            }
            _currentBoundingBoxObjects.Clear();
        }
    }
}