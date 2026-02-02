using UnityEngine;
using Force;
using System.Collections.Generic;


namespace smarc.genericControllers
{
    public enum AltitudeControlMode
    {
        AbsoluteAltitude,
        VerticalVelocity
    }

    [AddComponentMenu("Smarc/Generic Controllers/Altitude Controller")]
    public class AltitudeController : MonoBehaviour
    {
        public ArticulationBody RobotAB;
        public Rigidbody RobotRB;
        private MixedBody robotBody;

        public AltitudeControlMode ControlMode = AltitudeControlMode.AbsoluteAltitude;
        [Tooltip("If true, controller will add force to compensate for gravity, this makes the robot float by default")]
        public bool CompensateGravity = true;
        [Tooltip("Additional masses (in kg) to compensate for when computing required lift force")]
        public List<float> ExtraMassesToCompensateFor = new();

        public float AscentRate = 2.0f;
        public float DescentRate = 2.0f;
        [Tooltip("Set to 0 to disable force capping")]
        public float MaxForce = 0f;


        [Header("Velocity Settings")]
        public float TargetVelocity = 0.0f;

        [Header("Position Settings")]
        public float TargetAltitude = 10.0f;
        public float AltitudeTolerance = 0.1f;
        public float GroundLevel = 0f; 


        [Header("Velocity PID")]
        public float VelKp = 5.0f;
        public float VelKi = 0.0f;
        public float VelKd = 0.0f;
        public float VelIntegratorLimit = 5f; // limits integral term (in meter-seconds)
        PID velPID;




        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            velPID = new PID(VelKp, VelKi, VelKd, VelIntegratorLimit, maxOutput: MaxForce);
        }

        void FixedUpdate()
        {
            if (ControlMode == AltitudeControlMode.AbsoluteAltitude)
            {
                float diff = TargetAltitude - (robotBody.transform.position.y - GroundLevel);
                if (Mathf.Abs(diff) <= AltitudeTolerance) TargetVelocity = 0f;
                else TargetVelocity = Mathf.Sign(diff) * ((diff > 0) ? AscentRate : DescentRate);
            }
            VelocityControl();
        }

        float LimitAccelation(float desiredAcc, float currentVel, float deltaTime)
        {
            // limit acceleration so we don't instantly exceed configured ascent/descent rates
            float maxAccThisStepUp = (AscentRate - currentVel) / deltaTime;
            float maxAccThisStepDown = (-DescentRate - currentVel) / deltaTime;
            float minAcc = Mathf.Min(maxAccThisStepDown, maxAccThisStepUp);
            float maxAcc = Mathf.Max(maxAccThisStepDown, maxAccThisStepUp);
            return Mathf.Clamp(desiredAcc, minAcc, maxAcc);
        }

        float DownForce()
        {
            float totalMass = robotBody.mass;
            foreach (float extraMass in ExtraMassesToCompensateFor)
            {
                totalMass += extraMass;
            }
            return totalMass * -Physics.gravity.y; // Physics.gravity.y is negative for "down"
        }

        void VelocityControl()
        {
            float currentVel = robotBody.velocity.y;
            float pidAcc = velPID.Update(TargetVelocity, currentVel, Time.fixedDeltaTime);
            pidAcc = LimitAccelation(pidAcc, currentVel, Time.fixedDeltaTime);

            float requiredForce = CompensateGravity ? pidAcc + DownForce() : pidAcc;
            requiredForce = MaxForce > 0f ? Mathf.Clamp(requiredForce, -MaxForce, MaxForce) : requiredForce;

            Vector3 upForce = Vector3.up * requiredForce;
            robotBody.AddForceAtPosition(upForce, robotBody.transform.position, ForceMode.Force);
        }
        

        void OnDrawGizmosSelected()
        {
            // Draw target altitude line
            Gizmos.color = Color.green;
            Transform tf = this.transform;
            if(RobotAB != null)
                tf = RobotAB.transform;
            else if(RobotRB != null)
                tf = RobotRB.transform;
            Vector3 startPos = tf.position;
            Vector3 endPos = new(tf.position.x, GroundLevel + TargetAltitude, tf.position.z);
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}