using UnityEngine;
using Force;
using DefaultNamespace;
using UnityEditor.EditorTools;


namespace dji
{
    public enum YawControlMode
    {
        CompassHeading,
        YawRate
    }
    
    public class YawController : MonoBehaviour
    {
        public ArticulationBody RobotAB;
        public Rigidbody RobotRB;
        private MixedBody robotBody;
        public YawControlMode ControlMode = YawControlMode.CompassHeading;

        [Tooltip("Set to 0 to disable torque capping")]
        public float MaxTorque = 1f; // 0 = no explicit torque cap
        [Tooltip("Acceptable tolerance in degrees")]
        public float Tolerance = 2.0f;

        [Header("Yaw Rate Controller")]
        public float TargetYawRate = 0.0f; // Target yaw rate in degrees per second

        [Header("Compass Heading Controller")]
        public float TargetCompassHeading = 0.0f; // Target heading in degrees

        [Header("Yaw Rate PID")]
        public float YawRateKp = 0.1f;
        public float YawRateKi = 0.0f;
        public float YawRateKd = 0.0f;
        public float YawRateIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private PID yawRatePID;


        [Header("Heading PID")]
        public float HeadingKp = 2.0f;
        public float HeadingKi = 0.5f;
        public float HeadingKd = 1.0f;
        public float HeadingIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private PID headingPID;


        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            yawRatePID = new PID(YawRateKp, YawRateKi, YawRateKd, YawRateIntegratorLimit, Tolerance);
            headingPID = new PID(HeadingKp, HeadingKi, HeadingKd, HeadingIntegratorLimit, Tolerance);
        }

        void FixedUpdate()
        {
            if (ControlMode == YawControlMode.CompassHeading)
            {
                HeadingHold();
            }
            else if (ControlMode == YawControlMode.YawRate)
            {
                YawRateHold();
            }
        }
        
        void YawRateHold()
        {
            // Get current yaw rate in degrees per second
            float currentYawRate = robotBody.angularVelocity.y * Mathf.Rad2Deg;
            float torque = yawRatePID.Update(TargetYawRate, currentYawRate, Time.fixedDeltaTime);

            if (MaxTorque > 0f)
            {
                torque = Mathf.Clamp(torque, -MaxTorque, MaxTorque);
            }
            // Apply torque around the Y-axis (yaw)
            robotBody.AddTorque(new Vector3(0f, torque, 0f), ForceMode.Force);    
        }

        void HeadingHold()
        {
            float currentHeading = robotBody.transform.eulerAngles.y;
            // Compute shortest angle difference, so that PID doesn't try to spin the long way around
            float angleDifference = Mathf.DeltaAngle(currentHeading, TargetCompassHeading);
            float torque = headingPID.Update(angleDifference, 0f, Time.fixedDeltaTime);

            if (MaxTorque > 0f)
            {
                torque = Mathf.Clamp(torque, -MaxTorque, MaxTorque);
            }
            // Apply torque around the Y-axis (yaw)
            robotBody.AddTorque(new Vector3(0f, torque, 0f), ForceMode.Force);    
        }
    }
}