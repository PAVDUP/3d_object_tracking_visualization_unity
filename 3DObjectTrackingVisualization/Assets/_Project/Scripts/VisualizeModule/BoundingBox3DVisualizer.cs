using System.Collections.Generic;
using DataType;
using UnityEngine;

namespace VisualizeModule
{
    public class BoundingBox3DVisualizer : MonoBehaviour
    {
        public Material boundingBoxMaterial;
        public Transform cameraTransform; // 카메라의 Transform 참조!!!!!!!!!!!

        // 현재 활성화된 바운딩 박스 오브젝트들을 추적하기 위한 리스트
        private List<GameObject> _currentBoundingBoxObjects = new List<GameObject>();

        /// <summary>
        /// 주어진 바운딩 박스 데이터로 바운딩 박스를 시각화
        /// </summary>
        /// <param name="boundingBoxes">시각화할 바운딩 박스 데이터 리스트</param>
        public void VisualizeBoundingBoxes(List<BoundingBox3D> boundingBoxes)
        {
            ClearBoundingBoxes(); // 이전에 생성된 바운딩 박스를 제거

            foreach (var bbox in boundingBoxes)
            {
                // 카메라의 Transform을 적용하여 월드 좌표계 위치 계산
                Vector3 worldPosition = cameraTransform.TransformPoint(bbox.Center);
                Quaternion worldRotation = cameraTransform.rotation * bbox.Rotation;

                GameObject bboxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bboxObject.transform.position = worldPosition;
                bboxObject.transform.localScale = bbox.Size;
                bboxObject.transform.rotation = worldRotation;

                // 선택적으로 머티리얼 적용
                if (boundingBoxMaterial != null)
                {
                    Renderer renderer = bboxObject.GetComponent<Renderer>();
                    renderer.material = boundingBoxMaterial;
                }

                // 생성된 오브젝트를 리스트에 추가
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