using UnityEngine;

namespace DefaultNamespace
{
    class PID
    {
        float Kp;
        float Ki;
        float Kd;
        float IntegratorLimit;

        float integrator = 0f;
        float lastError = 0f;
        float tolerance = 0f;

        public PID(float kp, float ki, float kd, float integratorLimit, float tolerance = 0f)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            IntegratorLimit = integratorLimit;
            this.tolerance = tolerance;
        }

        public Vector3 UpdateVector3(Vector3 target, Vector3 current, float deltaTime)
        {
            Vector3 error = target - current;
            float mag = error.magnitude;
            if (mag < tolerance)
            {
                return Vector3.zero;
            }
            integrator += mag * deltaTime;
            integrator = Mathf.Clamp(integrator, -IntegratorLimit, IntegratorLimit);
            float derivative = (mag - lastError) / deltaTime;
            lastError = mag;
            return (Kp * error) + (Ki * integrator * error.normalized) + (Kd * derivative * error.normalized);
        }

        public float Update(float target, float current, float deltaTime)
        {
            float error = target - current;
            if (Mathf.Abs(error) < tolerance)
            {
                return 0f;
            }

            // Proportional term
            float P = Kp * error;

            // Integral term
            integrator += error * deltaTime;
            integrator = Mathf.Clamp(integrator, -IntegratorLimit, IntegratorLimit);
            float I = Ki * integrator;

            // Derivative term
            float derivative = (error - lastError) / deltaTime;
            float D = Kd * derivative;

            lastError = error;

            return P + I + D;
        }

        public void Reset()
        {
            integrator = 0f;
            lastError = 0f;
        }
    }

}