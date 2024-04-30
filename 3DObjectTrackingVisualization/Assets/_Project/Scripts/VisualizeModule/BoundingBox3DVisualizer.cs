using System.Collections.Generic;
using DataType;
using ScriptableObjectV;
using UnityEngine;

namespace VisualizeModule
{
    public class BoundingBox3DVisualizer : MonoBehaviour
    {
        public ClassificationPrefabs classificationPrefabsSetting;
        
        public Material boundingBoxMaterial;
        public Transform cameraTransform; // 카메라의 Transform 참조!!!!!!!!!!!

        // 현재 활성화된 바운딩 박스 추적
        private readonly List<GameObject> _currentBoundingBoxObjects = new List<GameObject>();
        
        private void Start()
        {
            if (cameraTransform == null)
            {
                Debug.LogError("[BoundingBox3DVisualizer] cameraTransform is not set. Please set the camera transform.");
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
        public void VisualizeBoundingBoxes(List<BoundingBox3D> boundingBoxes)
        {
            ClearBoundingBoxes(); 

            foreach (var bbox in boundingBoxes)
            {
                // 카메라의 Transform을 적용 -> 월드 좌표계 위치 계산
                Vector3 worldPosition = cameraTransform.TransformPoint(bbox.Center);
                Quaternion worldRotation = cameraTransform.rotation * bbox.Rotation;

                // 오브젝트 생성
                GameObject bboxObject = null;
                foreach (var vClassificationPrefab in classificationPrefabsSetting.classificationPrefabs)
                {
                    if (bbox.Classification == vClassificationPrefab.classificationType)
                    {
                        bboxObject = Instantiate(vClassificationPrefab.classificationPrefabCandidates[Random.Range(0, vClassificationPrefab.classificationPrefabCandidates.Length)]);
                        break;
                    }
                }
                
                if (bboxObject == null)
                {
                    bboxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (boundingBoxMaterial != null)
                        bboxObject.GetComponent<Renderer>().material = boundingBoxMaterial;
                }
                
                bboxObject.transform.position = worldPosition;
                bboxObject.transform.localScale = bbox.Size; // 바운딩 박스 크기를 여기서 설정하므로, Prefab들의 크기가 모두 동일해야 함. (1, 1, 1) 일때.
                bboxObject.transform.rotation = worldRotation;
                
                _currentBoundingBoxObjects.Add(bboxObject);
            }
        }

        /// <summary>
        /// 모든 바운딩 박스 오브젝트를 제거
        /// </summary>
        private void ClearBoundingBoxes()
        {
            foreach (var obj in _currentBoundingBoxObjects)
            {
                Destroy(obj);
            }
            _currentBoundingBoxObjects.Clear();
        }
    }
}