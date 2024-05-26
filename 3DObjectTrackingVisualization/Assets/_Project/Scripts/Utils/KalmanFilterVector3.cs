using UnityEngine;

namespace Utils
{
    public class KalmanFilterVector3
    {
        public Vector3 Estimate;
        private float _errorCovariance = 1;
        private readonly float _processNoise;
        private readonly float _measurementNoise;

        public KalmanFilterVector3(Vector3 initialEstimate, float processNoise = 0.1f, float measurementNoise = 0.1f)
        {
            Estimate = initialEstimate;
            _processNoise = processNoise;
            _measurementNoise = measurementNoise;
        }

        public Vector3 UpdateKalman(Vector3 measurement)
        {
            // Prediction Update
            Vector3 predEstimate = Estimate;
            float predErrorCovariance = _errorCovariance + _processNoise;

            // Kalman Gain
            float kalmanGain = predErrorCovariance / (predErrorCovariance + _measurementNoise);

            // Measurement Update
            Estimate = predEstimate + kalmanGain * (measurement - predEstimate);
            _errorCovariance = (1 - kalmanGain) * predErrorCovariance;

            return Estimate;
        }
    }
}