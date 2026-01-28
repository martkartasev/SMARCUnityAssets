using UnityEngine;
using Force;
using DefaultNamespace;


namespace dji
{
    public enum YawControlMode
    {
        CompassHeading,
        YawRate
    }

    public enum RollPitchMode
    {
        Upright,
        ReactToAcceleration
    }
    
    public class AttitudeController : MonoBehaviour
    {
        public ArticulationBody RobotAB;
        public Rigidbody RobotRB;
        private MixedBody robotBody;
        public YawControlMode ControlMode = YawControlMode.CompassHeading;
        public RollPitchMode RollPitchMode = RollPitchMode.Upright;


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

        [Header("Reactive Roll/Pitch Settings")]
        public HorizontalController horizontalController;
        public float MaxTiltAngle = 20f;
        public float ExpectedMaxAccel = 10f;
        public float Kp = 1.0f;
        



        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            yawRatePID = new PID(YawRateKp, YawRateKi, YawRateKd, YawRateIntegratorLimit, Tolerance);
            headingPID = new PID(HeadingKp, HeadingKi, HeadingKd, HeadingIntegratorLimit, Tolerance);
        }

        void FixedUpdate()
        {
            // check if the robot is upright enough to control
            var upDot = Vector3.Dot(robotBody.transform.up, Vector3.up);
            if (upDot < 0.5f)
            {
                Debug.Log($"Robot too tilted for yaw control! upDot: {upDot}");
                return;
            }

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