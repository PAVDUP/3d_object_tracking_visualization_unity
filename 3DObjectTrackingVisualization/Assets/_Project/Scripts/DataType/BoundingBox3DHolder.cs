using UnityEngine;
using Utils;

namespace DataType
{
    public class BoundingBox3DHolder : MonoBehaviour
    {
        public BoundingBox3D BoundingBox3D;
        private KalmanFilterVector3 _positionFilter;
        
        public void SetBoundingBox3DInfo(BoundingBox3D boundingBox3D)
        {
            BoundingBox3D = boundingBox3D;
        }
        
        void Awake()
        {
            _positionFilter = new KalmanFilterVector3(BoundingBox3D.center);
        }

        public void UpdateState(Vector3 newPosition, Quaternion newRotation)
        {
            Vector3 filteredPosition = _positionFilter.UpdateKalman(newPosition);

            BoundingBox3D.center = filteredPosition;
        }
    }
}
