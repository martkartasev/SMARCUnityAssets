using UnityEngine;
using Force;


namespace dji
{
    public enum AltitudeControlMode
    {
        PositionHold,
        FLU_Velocity
    }
    public class AltitudeController : MonoBehaviour
    {
        public ArticulationBody droneAB;
        public Rigidbody droneRB;
        private MixedBody droneBody;
        public AltitudeControlMode controlMode = AltitudeControlMode.PositionHold;

        public float AscentRate = 2.0f; // Ascent rate in meters per second
        public float DescentRate = 2.0f; // Descent rate in meters per second
        public float MaxForce = 0f; // 0 = no explicit force cap


        [Header("FLU Velocity Settings")]
        public float TargetFLUVelocity = 0.0f; // Target vertical velocity in m/s (positive = up, negative = down)

        [Header("Position Hold Settings")]
        public float TargetAltitude = 10.0f; // Target altitude in meters
        public float AltitudeTolerance = 0.5f; // Acceptable altitude tolerance in meters
        public float GroundLevel = 0f; 


        [Header("Velocity PID")]
        public float VelKp = 5.0f;
        public float VelKi = 0.0f;
        public float VelKd = 0.0f;
        private float velIntegrator = 0f;
        private float velLastError = 0f;


        [Header("Position PID")]
        public float PosKp = 2.0f;
        public float PosKi = 0.5f;
        public float PosKd = 1.0f;
        public float PosIntegratorLimit = 5f; // limits integral term (in meter-seconds)
        private float posIntegrator = 0f;
        private float posLastError = 0f;



        void Start()
        {
            droneBody = new MixedBody(droneAB, droneRB);
        }

        // PID tuning
        void FixedUpdate()
        {
            if (controlMode == AltitudeControlMode.PositionHold)
            {
                PositionHold();
            }
            else if (controlMode == AltitudeControlMode.FLU_Velocity)
            {
                FLUVelocityControl();
            }
        }

        void FLUVelocityControl()
        {
            {
                float dt = Time.fixedDeltaTime;
                if (dt <= 0f) return;

                // current vertical velocity (world up)
                float currentVel = droneBody.velocity.y;

                // PID error (target - current), positive means accelerate up
                float velError = TargetFLUVelocity - currentVel;

                // reuse velIntegrator/velLastError for velocity PID state
                velIntegrator += velError * dt;
                velIntegrator = Mathf.Clamp(velIntegrator, -PosIntegratorLimit, PosIntegratorLimit);

                float derivative = (velError - velLastError) / dt;

                // PID output is desired vertical acceleration (m/s^2)
                float pidAcc = VelKp * velError + VelKi * velIntegrator + VelKd * derivative;

                // limit acceleration so we don't instantly exceed configured ascent/descent rates
                float maxAccThisStepUp = (AscentRate - currentVel) / dt;
                float maxAccThisStepDown = (-DescentRate - currentVel) / dt;
                float minAcc = Mathf.Min(maxAccThisStepDown, maxAccThisStepUp);
                float maxAcc = Mathf.Max(maxAccThisStepDown, maxAccThisStepUp);
                pidAcc = Mathf.Clamp(pidAcc, minAcc, maxAcc);

                // Compensate gravity: required force = m * (desiredAccel - gravity)
                float g = Physics.gravity.y; // negative
                float requiredForce = droneBody.mass * (pidAcc - g);

                if (MaxForce > 0f)
                {
                    requiredForce = Mathf.Clamp(requiredForce, -MaxForce, MaxForce);
                }

                Vector3 upForce = Vector3.up * requiredForce;
                droneBody.AddForceAtPosition(upForce, droneBody.transform.position, ForceMode.Force);

                velLastError = velError;
            }
        }
        

        void PositionHold()
        {
            float currentAltitude = droneBody.transform.position.y - GroundLevel;
            float posError = TargetAltitude - currentAltitude;
            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            float currentVel = droneBody.velocity.y;

            // If within tolerance, reset integrator and hold hover (PID accel = 0)
            float pidAcc = 0f;
            if (Mathf.Abs(posError) > AltitudeTolerance)
            {
                // integrate with anti-windup (clamp)
                posIntegrator += posError * dt;
                posIntegrator = Mathf.Clamp(posIntegrator, -PosIntegratorLimit, PosIntegratorLimit);

                // derivative
                float derivative = (posError - posLastError) / dt;

                // PID output is an acceleration setpoint (m/s^2)
                pidAcc = PosKp * posError + PosKi * posIntegrator + PosKd * derivative;
            }
            else
            {
                posIntegrator = 0f;
            }

            // limit acceleration so we don't instantly exceed configured ascent/descent rates
            float maxAccThisStepUp = (AscentRate - currentVel) / dt;           // how much accel we can add without exceeding ascentRate
            float maxAccThisStepDown = (-DescentRate - currentVel) / dt;      // how much accel we can add (typically negative) without exceeding descentRate
            float minAcc = Mathf.Min(maxAccThisStepDown, maxAccThisStepUp);
            float maxAcc = Mathf.Max(maxAccThisStepDown, maxAccThisStepUp);
            pidAcc = Mathf.Clamp(pidAcc, minAcc, maxAcc);

            // Compensate gravity: required force = m * (desiredAccel - gravity)
            float g = Physics.gravity.y; // negative (e.g. -9.81)
            float requiredForce = droneBody.mass * (pidAcc - g);

            // optional force clamp
            if (MaxForce > 0f)
            {
                requiredForce = Mathf.Clamp(requiredForce, -MaxForce, MaxForce);
            }

            Vector3 upForce = Vector3.up * requiredForce;
            droneBody.AddForceAtPosition(upForce, droneBody.transform.position, ForceMode.Force);

            posLastError = posError;
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