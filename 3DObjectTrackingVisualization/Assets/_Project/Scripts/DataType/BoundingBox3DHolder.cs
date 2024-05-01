using UnityEngine;

namespace DataType
{
    public class BoundingBox3DHolder : MonoBehaviour
    {
        public BoundingBox3D BoundingBox3D;
        
        public void SetBoundingBox3DInfo(BoundingBox3D boundingBox3D)
        {
            BoundingBox3D = boundingBox3D;
        }
    }
}
