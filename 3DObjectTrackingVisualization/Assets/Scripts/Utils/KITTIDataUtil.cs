using System;
using System.Globalization;
using System.Linq;
using DataType;
using UnityEngine;

namespace Utils
{
    public static class KITTIDataUtil
    {
        public static CalibrationData ParseCalibration(string[] calibLines)
        {
            var calibrationData = new CalibrationData();
            foreach (var line in calibLines)
            {
                var parts = line.Split(':');
                var key = parts[0].Trim();
                var values = parts[1].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(val => float.Parse(val, CultureInfo.InvariantCulture.NumberFormat))
                    .ToArray();

                switch (key)
                {
                    case "P2":
                        calibrationData.CameraMatrix = BuildMatrix4X4(values, 3, 4); // P2는 3x4 크기입니다.
                        break;
                    case "R0_rect":
                        calibrationData.RectificationMatrix = BuildMatrix4X4(values, 3, 3, true); // R0_rect는 3x3 크기이며, 4x4 행렬로 확장해야 합니다.
                        break;
                    case "Tr_velo_to_cam":
                        calibrationData.TrVeloToCam = BuildMatrix4X4(values, 3, 4); // Tr_velo_to_cam은 3x4 크기입니다.
                        break;
                }
            }
            return calibrationData;
        }

        private static Matrix4x4 BuildMatrix4X4(float[] values, int rows, int cols, bool extendTo4X4 = false)
        {
            var matrix = new Matrix4x4();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = values[i * cols + j];
                }
            }
            if (extendTo4X4 && rows == 3 && cols == 3)
            {
                // R0_rect를 4x4 행렬로 확장
                matrix[3, 3] = 1f;
            }
            return matrix;
        }

        
        public static BoundingBox3D TransformBoundingBox(BoundingBox3D box, Matrix4x4 trMatrix)
        {
            // 바운딩 박스의 중심점 변환
            Vector3 transformedCenter = trMatrix.MultiplyPoint3x4(box.Center);

            // 바운딩 박스 회전 적용
            // 이 예제에서는 간단히 처리를 위해 Quaternion을 직접 사용합니다.
            // 실제로는 Tr_velo_to_cam 행렬의 회전 부분을 Quaternion으로 변환해야 할 수 있습니다.
            var rotation = Quaternion.LookRotation(
                trMatrix.GetColumn(2),  // Forward
                trMatrix.GetColumn(1)   // Up
            );

            // 바운딩 박스의 크기는 변환 행렬에 영향을 받지 않습니다.
            // 변환 행렬에 따라 크기가 변하는 경우가 있으나, 여기서는 처리하지 않습니다.
            return new BoundingBox3D(transformedCenter, box.Size, rotation);
        }
        
        public static Quaternion RotationFromYaw(float yawDegrees)
        {
            // Yaw 값을 Quaternion으로 변환합니다.
            // Yaw 값은 도 단위로 주어지며, Unity의 회전은 Y 축을 기준으로 합니다.
            return Quaternion.Euler(0, yawDegrees, 0);
        }
    }
}