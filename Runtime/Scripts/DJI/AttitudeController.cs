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
        public YawControlMode YawControlMode = YawControlMode.CompassHeading;
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

        [Header("RollPitch Controller")]
        public Vector3 TargetUp = Vector3.up;

        [Header("Yaw Rate PID")]
        public float YawRateKp = 0.1f;
        public float YawRateKi = 0.0f;
        public float YawRateKd = 0.0f;
        public float YawRateIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private PID yawRatePID;


        [Header("RollPitch PID")]
        public float RollPitchKp = 1.0f;
        public float RollPitchKi = 0.0f;
        public float RollPitchKd = 0.0f;
        public float RollPitchIntegratorLimit = 10f; // limits integral term (in degree-seconds)
        private PID rollPitchPID;


        [Header("Reactive Roll/Pitch Settings")]
        public HorizontalController horizontalController;
        public float MaxTiltAngle = 20f;
        public float ExpectedMaxAccel = 10f;
        public float Kp = 1.0f;

        



        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            yawRatePID = new PID(YawRateKp, YawRateKi, YawRateKd, YawRateIntegratorLimit, Tolerance, MaxTorque);
            rollPitchPID = new PID(RollPitchKp, RollPitchKi, RollPitchKd, RollPitchIntegratorLimit, Tolerance, MaxTorque);
        }

        void FixedUpdate()
        {
            float upDotLimit = 0.5f;
            var upDot = Vector3.Dot(robotBody.transform.up, Vector3.up);
            if (RollPitchMode == RollPitchMode.Upright || upDot >= upDotLimit)
            {                 
                Upright();
            }


            // check if the robot is upright enough to control the yaw of
            if (upDot < upDotLimit)
            {
                Debug.Log($"Robot too tilted for yaw control! upDot: {upDot}");
                return;
            }

            if (YawControlMode == YawControlMode.CompassHeading)
            {
                float currentHeading = robotBody.transform.eulerAngles.y;
                // Compute shortest angle difference, so that PID doesn't try to spin the long way around
                float angleDifference = Mathf.DeltaAngle(currentHeading, TargetCompassHeading);
                if (Mathf.Abs(angleDifference) <= Tolerance) TargetYawRate = 0f;
                else TargetYawRate = Mathf.Sign(angleDifference) * DesiredYawRate;
            }
            YawRateControl();
        }

        void Upright()
        {
            // Angle and axis to rotate current  -> target
            Vector3 currentUp = robotBody.transform.up;
            Vector3 axis = Vector3.Cross(currentUp, TargetUp);

            // Handle nearly-parallel or anti-parallel vectors robustly
            if (axis.sqrMagnitude < 1e-6f)
            {
                float dot = Vector3.Dot(currentUp, TargetUp);
                if (dot > 0f)
                {
                    // Already aligned (or numerically very close)
                    axis = Vector3.zero;
                }
                else
                {
                    // Opposite direction (180°): pick a stable perpendicular axis
                    axis = Vector3.Cross(currentUp, robotBody.transform.forward);
                    if (axis.sqrMagnitude < 1e-6f)
                        axis = Vector3.Cross(currentUp, robotBody.transform.right);

                    // Ensure axis points so a small positive rotation moves currentUp toward TargetUp
                    if (Vector3.Dot(Quaternion.AngleAxis(1f, axis.normalized) * currentUp, TargetUp) <
                        Vector3.Dot(currentUp, TargetUp))
                    {
                        axis = -axis;
                    }
                }
            }
            else
            {
                // Ensure axis direction reduces the angle (choose shortest rotation sign)
                if (Vector3.Dot(Quaternion.AngleAxis(1f, axis.normalized) * currentUp, TargetUp) <
                    Vector3.Dot(currentUp, TargetUp))
                {
                    axis = -axis;
                }
            }
            float axisMag = axis.magnitude;
            if (axisMag < 1e-6f) return; // already aligned or numerically unstable

            float dot2 = Mathf.Clamp(Vector3.Dot(robotBody.transform.up, TargetUp), -1f, 1f);
            float errorDeg = Mathf.Acos(dot2) * Mathf.Rad2Deg;

            // Proportional torque around the corrective axis (world space)
            Vector3 correctiveAxis = axis / axisMag;
            float torqueMag = rollPitchPID.Update(0f, -errorDeg, Time.fixedDeltaTime);
            Vector3 torque = correctiveAxis * torqueMag;

            // Apply torque to right the robot (apply in world space)
            robotBody.AddTorque(torque, ForceMode.Force);
        }
        
        void YawRateControl()
        {
            // Get current yaw rate in degrees per second
            float currentYawRate = robotBody.angularVelocity.y * Mathf.Rad2Deg;
            float torque = yawRatePID.Update(TargetYawRate, currentYawRate, Time.fixedDeltaTime);
            // Apply torque around the Y-axis (yaw)
            robotBody.AddTorque(new Vector3(0f, torque, 0f), ForceMode.Force);    
        }

        void OnDrawGizmosSelected()
        {
            // Draw target attitude line
            Gizmos.color = Color.green;
            Transform tf = this.transform;
            if(RobotAB != null)
                tf = RobotAB.transform;
            else if(RobotRB != null)
                tf = RobotRB.transform;
            Vector3 startPos = tf.position;
            Vector3 endPos = tf.position + TargetUp.normalized * 2.0f;
            Gizmos.DrawLine(startPos, endPos);

            // Draw target yaw direction
            if(YawControlMode == YawControlMode.CompassHeading)
            {
                Gizmos.color = Color.blue;
                Vector3 yawDir = Quaternion.Euler(0f, TargetCompassHeading, 0f) * Vector3.forward;
                Vector3 yawEndPos = tf.position + yawDir.normalized * 2.0f;
                Gizmos.DrawLine(startPos, yawEndPos);
            }
            if(YawControlMode == YawControlMode.YawRate)
            {
                Gizmos.color = Color.blue;
                Vector3 yawDir = Quaternion.Euler(0f, robotBody.transform.eulerAngles.y + TargetYawRate, 0f) * Vector3.forward;
                Vector3 yawEndPos = tf.position + yawDir.normalized * 2.0f;
                Gizmos.DrawLine(startPos, yawEndPos);
            }

        }
    }
}