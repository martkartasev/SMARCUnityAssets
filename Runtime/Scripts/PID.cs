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

        public float Update(float target, float current, float deltaTime)
        {
            float error = target - current;
            Debug.Log("PID Error: " + error);
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

    class PIDVector3
    {
        public PID X;
        public PID Y;
        public PID Z;

        public PIDVector3(float kp, float ki, float kd, float integratorLimit, float tolerance = 0f)
        {
            X = new PID(kp, ki, kd, integratorLimit, tolerance);
            Y = new PID(kp, ki, kd, integratorLimit, tolerance);
            Z = new PID(kp, ki, kd, integratorLimit, tolerance);
        }

        public Vector3 Update(Vector3 target, Vector3 current, float deltaTime)
        {
            float xOutput = X.Update(target.x, current.x, deltaTime);
            float yOutput = Y.Update(target.y, current.y, deltaTime);
            float zOutput = Z.Update(target.z, current.z, deltaTime);

            return new Vector3(xOutput, yOutput, zOutput);
        }

        public void Reset()
        {
            X.Reset();
            Y.Reset();
            Z.Reset();
        }
    }
}