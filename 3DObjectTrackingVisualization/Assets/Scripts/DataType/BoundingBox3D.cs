using UnityEngine;

namespace DataType
{
    public struct BoundingBox3D
    {
        public Vector3 Center; 
        public Vector3 Size; 
        public Quaternion Rotation; // y axis (up) rotation. 
        
        public BoundingBox3D(Vector3 center, Vector3 size, Quaternion rotation)
        {
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
}