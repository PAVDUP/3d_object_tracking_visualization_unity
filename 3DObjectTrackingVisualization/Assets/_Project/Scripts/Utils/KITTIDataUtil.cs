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
                if (parts.Length < 2) continue; // ':'을 기준으로 분할한 결과가 2개 미만이면 건너뜁니다.

                var key = parts[0].Trim();
                var valueString = parts[1].Trim();
                var values = valueString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(val => float.Parse(val, CultureInfo.InvariantCulture))
                                .ToArray();

                switch (key)
                {
                    case "P2":
                        calibrationData.CameraMatrix = BuildMatrix4x3(values);
                        break;
                    case "R0_rect":
                        calibrationData.RectificationMatrix = BuildMatrix3x3(values);
                        break;
                    case "Tr_velo_to_cam":
                        calibrationData.TrVeloToCam = BuildMatrix4x3(values);
                        break;
                }
            }
            return calibrationData;
        }

        // 3x4 매트릭스를 생성하는 함수
        private static Matrix4x4 BuildMatrix4x3(float[] values)
        {
            if (values.Length < 12) return Matrix4x4.identity; // 값이 충분하지 않은 경우 단위 매트릭스를 반환

            var matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(values[0], values[1], values[2], values[3]));
            matrix.SetRow(1, new Vector4(values[4], values[5], values[6], values[7]));
            matrix.SetRow(2, new Vector4(values[8], values[9], values[10], values[11]));
            matrix.SetRow(3, new Vector4(0, 0, 0, 1)); // 4번째 줄을 (0, 0, 0, 1)로 설정하여 확장합니다.
            return matrix;
        }

        // 3x3 매트릭스를 생성하고 4x4로 확장하는 함수
        private static Matrix4x4 BuildMatrix3x3(float[] values)
        {
            if (values.Length < 9) return Matrix4x4.identity; // 값이 충분하지 않은 경우 단위 매트릭스를 반환

            var matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(values[0], values[1], values[2], 0));
            matrix.SetRow(1, new Vector4(values[3], values[4], values[5], 0));
            matrix.SetRow(2, new Vector4(values[6], values[7], values[8], 0));
            matrix.SetRow(3, new Vector4(0, 0, 0, 1)); // 4번째 줄을 (0, 0, 0, 1)로 설정하여 확장합니다.
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
            return new BoundingBox3D(box.RawClassificationData, transformedCenter, box.Size, rotation);
        }
        
        public static Quaternion RotationFromYaw(float yawDegrees)
        {
            // Yaw 값을 Quaternion으로 변환합니다.
            // Yaw 값은 도 단위로 주어지며, Unity의 회전은 Y 축을 기준으로 합니다.
            return Quaternion.Euler(0, yawDegrees, 0);
        }
    }
}