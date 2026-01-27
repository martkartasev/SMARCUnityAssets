using UnityEngine;
using Force;
using DefaultNamespace;


namespace dji
{
    public enum AltitudeControlMode
    {
        AbsoluteAltitude,
        VerticalVelocity
    }
    public class AltitudeController : MonoBehaviour
    {
        public ArticulationBody droneAB;
        public Rigidbody droneRB;
        private MixedBody droneBody;
        public AltitudeControlMode controlMode = AltitudeControlMode.AbsoluteAltitude;

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


        [Header("Position PID")]
        public float PosKp = 2.0f;
        public float PosKi = 0.5f;
        public float PosKd = 1.0f;
        public float PosIntegratorLimit = 5f; // limits integral term (in meter-seconds)
        PID posPID;



        void Start()
        {
            droneBody = new MixedBody(droneAB, droneRB);
            velPID = new PID(VelKp, VelKi, VelKd, VelIntegratorLimit);
            posPID = new PID(PosKp, PosKi, PosKd, PosIntegratorLimit, AltitudeTolerance);
        }

        void FixedUpdate()
        {
            if (controlMode == AltitudeControlMode.AbsoluteAltitude)
            {
                PositionHold();
            }
            else if (controlMode == AltitudeControlMode.VerticalVelocity)
            {
                VelocityControl();
            }
        }

        float limitAccelation(float desiredAcc, float currentVel, float deltaTime)
        {
            // limit acceleration so we don't instantly exceed configured ascent/descent rates
            float maxAccThisStepUp = (AscentRate - currentVel) / deltaTime;
            float maxAccThisStepDown = (-DescentRate - currentVel) / deltaTime;
            float minAcc = Mathf.Min(maxAccThisStepDown, maxAccThisStepUp);
            float maxAcc = Mathf.Max(maxAccThisStepDown, maxAccThisStepUp);
            return Mathf.Clamp(desiredAcc, minAcc, maxAcc);
        }

        void VelocityControl()
        {
            float currentVel = droneBody.velocity.y;
            float pidAcc = velPID.Update(TargetVelocity, currentVel, Time.fixedDeltaTime);
            pidAcc = limitAccelation(pidAcc, currentVel, Time.fixedDeltaTime);

            // Compensate gravity: required force = m * (desiredAccel - gravity)
            float g = Physics.gravity.y; // negative
            float requiredForce = droneBody.mass * (pidAcc - g);

            if (MaxForce > 0f)
            {
                requiredForce = Mathf.Clamp(requiredForce, -MaxForce, MaxForce);
            }

            Vector3 upForce = Vector3.up * requiredForce;
            droneBody.AddForceAtPosition(upForce, droneBody.transform.position, ForceMode.Force);
        }
        

        void PositionHold()
        {
            float currentAltitude = droneBody.transform.position.y - GroundLevel;
            float pidAcc = posPID.Update(TargetAltitude, currentAltitude, Time.fixedDeltaTime);

            if(pidAcc <= 0f) posPID.Reset();

            pidAcc = limitAccelation(pidAcc, droneBody.velocity.y, Time.fixedDeltaTime);

            // Compensate gravity: required force = m * (desiredAccel - gravity)
            float g = Physics.gravity.y; // negative (e.g. -9.81)
            float requiredForce = droneBody.mass * (pidAcc - g);

            if (MaxForce > 0f)
            {
                requiredForce = Mathf.Clamp(requiredForce, -MaxForce, MaxForce);
            }

            Vector3 upForce = Vector3.up * requiredForce;
            droneBody.AddForceAtPosition(upForce, droneBody.transform.position, ForceMode.Force);
        }

        void OnDrawGizmosSelected()
        {
            // Draw target altitude line
            Gizmos.color = Color.green;
            Transform tf = this.transform;
            if(droneAB != null)
                tf = droneAB.transform;
            else if(droneRB != null)
                tf = droneRB.transform;
            Vector3 startPos = tf.position;
            Vector3 endPos = new(tf.position.x, GroundLevel + TargetAltitude, tf.position.z);
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}