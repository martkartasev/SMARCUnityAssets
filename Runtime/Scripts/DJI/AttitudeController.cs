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
        public float DesiredYawRate = 10f;

        [Header("Yaw Rate PID")]
        public float YawRateKp = 0.1f;
        public float YawRateKi = 0.0f;
        public float YawRateKd = 0.0f;
        public float YawRateIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private PID yawRatePID;


        [Header("Reactive Roll/Pitch Settings")]
        public HorizontalController horizontalController;
        public float MaxTiltAngle = 20f;
        public float ExpectedMaxAccel = 10f;
        public float Kp = 1.0f;
        



        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            yawRatePID = new PID(YawRateKp, YawRateKi, YawRateKd, YawRateIntegratorLimit, Tolerance, MaxTorque);
        }

        void FixedUpdate()
        {
            float upDotLimit = 0.5f;
            var upDot = Vector3.Dot(robotBody.transform.up, Vector3.up);
            if (RollPitchMode == RollPitchMode.Upright || upDot >= upDotLimit)
            {                 
                Upright();
            }
            // check if the robot is upright enough to control
            if (upDot < upDotLimit)
            {
                Debug.Log($"Robot too tilted for yaw control! upDot: {upDot}");
                return;
            }

            if (ControlMode == YawControlMode.CompassHeading)
            {
                float currentHeading = robotBody.transform.eulerAngles.y;
                // Compute shortest angle difference, so that PID doesn't try to spin the long way around
                float angleDifference = Mathf.DeltaAngle(currentHeading, TargetCompassHeading);
                if (Mathf.Abs(angleDifference) <= Tolerance) TargetYawRate = 0f;
                else TargetYawRate = Mathf.Sign(angleDifference) * DesiredYawRate;
            }
            YawRateHold();
        }

        void Upright()
        {
            
        }
        
        void YawRateHold()
        {
            // Get current yaw rate in degrees per second
            float currentYawRate = robotBody.angularVelocity.y * Mathf.Rad2Deg;
            float torque = yawRatePID.Update(TargetYawRate, currentYawRate, Time.fixedDeltaTime);
            // Apply torque around the Y-axis (yaw)
            robotBody.AddTorque(new Vector3(0f, torque, 0f), ForceMode.Force);    
        }
    }
}