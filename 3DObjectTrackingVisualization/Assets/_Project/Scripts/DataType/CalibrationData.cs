using UnityEngine;

namespace DataType
{
    public struct CalibrationData
    {
        public Matrix4x4 CameraMatrix; 
        public Matrix4x4 ProjectionMatrix; // 투영 행렬
        public Matrix4x4 RectificationMatrix; // 직사각화 행렬
        public Matrix4x4 TrVeloToCam; // 라이다 포인트 클라우드를 카메라 좌표계로 변환하는 행렬

        public CalibrationData(Matrix4x4 cameraMatrix, Matrix4x4 projectionMatrix, Matrix4x4 rectificationMatrix, Matrix4x4 trVeloToCam)
        {
            CameraMatrix = cameraMatrix;
            ProjectionMatrix = projectionMatrix;
            RectificationMatrix = rectificationMatrix;
            TrVeloToCam = trVeloToCam;
        }
        
        public static Matrix4x4 ParseMatrix(string data)
        {
            var entries = data.Split(' ');
            Matrix4x4 matrix = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                matrix[i] = float.Parse(entries[i]);
            }
            return matrix;
        }
    }
}