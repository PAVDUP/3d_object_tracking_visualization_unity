using UnityEngine;

namespace DataType
{
    public struct BoundingBox3D
    {
        public string RawClassificationData;
        public Vector3 Center; 
        public Vector3 Size; 
        public Quaternion Rotation; // y axis (up) rotation. 
        
        public BoundingBox3D(string rawClassificationData, Vector3 center, Vector3 size, Quaternion rotation)
        {
            RawClassificationData = rawClassificationData;
            Center = center;
            Size = size;
            Rotation = rotation;
        }

        /// <summary>
        /// KITTI dataset radian -> Quaternion
        /// </summary>
        /// <param name="yaw"></param>
        /// <returns></returns>
        public static Quaternion RotationFromYaw(float yaw)
        {
            return Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
        }
    }
    
    /// <summary>
    /// KITTI dataset classification 을 기반으로 한 BoundingBox3DType
    /// </summary>
    public enum BoundingBox3DType
    {
        Car,
        Pedestrian,
        Cyclist,
        Van,
        Truck,
        // Tran, Misc, DontCare 는 사용하지 않음
        Tram,
        Misc,
        DontCare
    }
}