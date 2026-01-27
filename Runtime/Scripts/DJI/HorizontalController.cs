using UnityEngine;
using Force;
using DefaultNamespace;


namespace dji
{
    public enum HorizontalControlMode
    {
        UnityPosition,
        Velocity
    }

    public class HorizontalController : MonoBehaviour
    {
        public ArticulationBody droneAB;
        public Rigidbody droneRB;
        private MixedBody droneBody;
        public HorizontalControlMode controlMode = HorizontalControlMode.UnityPosition;
        public float MaxForce = 0f; // 0 = no explicit force cap
        public float MaxSpeed = 5.0f; // max horizontal speed in m/s

        [Header("Velocity Controller")]
        public Vector3 TargetVelocity = Vector3.zero; // Target velocity in m/s

        [Header("Unity Position Controller")]
        public Vector3 TargetUnityPosition = Vector3.zero; // Target position in meters
        public float PositionTolerance = 0.5f; // Acceptable position tolerance in meters


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
            posPID = new PID(PosKp, PosKi, PosKd, PosIntegratorLimit);
        }

        void FixedUpdate()
        {
            TargetVelocity.y = 0;
            if (controlMode == HorizontalControlMode.UnityPosition)
            {
                PositionHold();
            }
            else if (controlMode == HorizontalControlMode.Velocity)
            {
                VelocityControl();
            }
        }

        void VelocityControl()
        {
            Vector3 currentVelocity = droneBody.transform.InverseTransformVector(droneBody.velocity);
            currentVelocity.y = 0;

            Vector3 force = velPID.UpdateVector3(TargetVelocity, currentVelocity, Time.fixedDeltaTime);
            force.y = 0;

            if (MaxForce > 0f && force.magnitude > MaxForce)
            {
                force = force.normalized * MaxForce;
            }

            droneBody.AddForceAtPosition(force, droneBody.transform.position, ForceMode.Force);
        }

        void PositionHold()
        {
            Vector3 currentPosition = droneBody.transform.position;
            currentPosition.y = 0;
            Vector3 targetPos = TargetUnityPosition;
            targetPos.y = 0;

            Vector3 force = posPID.UpdateVector3(targetPos, currentPosition, Time.fixedDeltaTime);
            force.y = 0;

            if (MaxForce > 0f && force.magnitude > MaxForce)
            {
                force = force.normalized * MaxForce;
            }

            droneBody.AddForceAtPosition(force, droneBody.transform.position, ForceMode.Force);
        }


    }
}
