using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataType
{
    [Serializable]
    public enum BoundingBoxCameraType
    {
        Front,
        Back
    }
    
    [Serializable]
    public struct BoundingBox3D
    {
        public BoundingBoxCameraType cameraType;
        public string rawClassificationData;
        public BoundingBox3DType classification;
        public int identifier;
        public Vector3 center; // Camera Transform 기반 Center
        public Vector3 size; 
        public Quaternion rotation; // y axis (up) rotation. 
        
        public BoundingBox3D(BoundingBoxCameraType inputCameraType, string rawClassificationData, BoundingBox3DType inputClassification, int identifier, Vector3 center, Vector3 size, Quaternion rotation)
        {
            cameraType = inputCameraType;
            this.rawClassificationData = rawClassificationData;
            classification = inputClassification;
            this.identifier = identifier;
            this.center = center;
            this.size = size;
            this.rotation = rotation;
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
    [Serializable]
    public enum BoundingBox3DType
    {
        Car,
        Pedestrian,
        Van,
        Truck,
        // Cyclist, Tran, Misc, DontCare 는 사용하지 않음
        Cyclist,
        Tram,
        Misc,
        DontCare
    }
}