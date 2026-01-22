using UnityEngine;
using Force;


namespace dji
{
    public enum YawControlMode
    {
        ENUHeading,
        YawRate
    }
    
    public class YawController : MonoBehaviour
    {
        public ArticulationBody droneAB;
        public Rigidbody droneRB;
        private MixedBody droneBody;
        public YawControlMode controlMode = YawControlMode.ENUHeading;
        public float MaxTorque = 1f; // 0 = no explicit torque cap

        [Header("Yaw Rate Controller")]
        public float TargetYawRate = 0.0f; // Target yaw rate in degrees per second

        [Header("ENU Heading Controller")]
        public float TargetENUHeading = 0.0f; // Target heading in degrees
        public float HeadingTolerance = 2.0f; // Acceptable heading tolerance in degrees

        [Header("Yaw Rate PID")]
        public float YawRateKp = 0.1f;
        public float YawRateKi = 0.0f;
        public float YawRateKd = 0.0f;
        public float YawRateIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private float yawRateIntegrator = 0f;
        private float yawRateLastError = 0f;

        [Header("Heading PID")]
        public float HeadingKp = 2.0f;
        public float HeadingKi = 0.5f;
        public float HeadingKd = 1.0f;
        public float HeadingIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private float headingIntegrator = 0f;
        private float headingLastError = 0f;

        void Start()
        {
            droneBody = new MixedBody(droneAB, droneRB);
        }

        void FixedUpdate()
        {
            Debug.Log(droneBody.transform.InverseTransformVector(droneBody.angularVelocity).y * Mathf.Rad2Deg);
            if (controlMode == YawControlMode.ENUHeading)
            {
                HeadingHold();
            }
            else if (controlMode == YawControlMode.YawRate)
            {
                YawRateHold();
            }
        }
        
        void YawRateHold()
        {
            // Get current yaw rate in degrees per second
            // float currentYawRate = droneBody.angularVelocity.y * Mathf.Rad2Deg;
            float currentYawRate = droneBody.transform.InverseTransformVector(droneBody.angularVelocity).y * Mathf.Rad2Deg;

            // Calculate error
            float error = TargetYawRate - currentYawRate;

            // Proportional term
            float P = YawRateKp * error;

            // Integral term
            yawRateIntegrator += error * Time.fixedDeltaTime;
            yawRateIntegrator = Mathf.Clamp(yawRateIntegrator, -YawRateIntegratorLimit, YawRateIntegratorLimit);
            float I = YawRateKi * yawRateIntegrator;

            // Derivative term
            float D = YawRateKd * (error - yawRateLastError) / Time.fixedDeltaTime;
            yawRateLastError = error;

            // Compute total torque
            float totalTorque = P + I + D;

            // Apply torque limit if specified
            if (MaxTorque > 0f)
            {
                totalTorque = Mathf.Clamp(totalTorque, -MaxTorque, MaxTorque);
            }
            // Apply torque around the Y-axis (yaw)
            droneBody.AddTorque(new Vector3(0f, totalTorque, 0f), ForceMode.Force);    
        }

        void HeadingHold()
        {
            
        }
    }
}