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
        public ArticulationBody RobotAB;
        public Rigidbody RobotRB;
        MixedBody robotBody;
        public HorizontalControlMode ControlMode = HorizontalControlMode.UnityPosition;
        public float MaxForce = 0f;
        public float MaxSpeed = 5.0f;

        [Header("Velocity Controller")]
        public Vector3 TargetVelocity = Vector3.zero;

        [Header("Unity Position Controller")]
        public Vector3 TargetUnityPosition = Vector3.zero;
        public float PositionTolerance = 0.5f;


        [Header("Velocity PID")]
        public float VelKp = 5.0f;
        public float VelKi = 0.0f;
        public float VelKd = 0.0f;
        public float VelIntegratorLimit = 5f;
        PID velPID;
        
        public Vector3 LastAppliedForce { get; private set; }


        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
            velPID = new PID(VelKp, VelKi, VelKd, VelIntegratorLimit, maxOutput:MaxForce);

            // set to current position so it doesnt try to fly away to (usually) origin lol
            if (ControlMode == HorizontalControlMode.UnityPosition)
            {
                TargetUnityPosition = robotBody.transform.position;
            }
        }

        void FixedUpdate()
        {
            if (ControlMode == HorizontalControlMode.UnityPosition)
            {
                Vector3 diff = TargetUnityPosition - robotBody.transform.position;
                if (diff.magnitude <= PositionTolerance) TargetVelocity = Vector3.zero;
                else TargetVelocity = diff.normalized * MaxSpeed;
            }
            TargetVelocity.y = 0;
            VelocityControl();
        }

        void VelocityControl()
        {
            Vector3 currentVelocity = robotBody.transform.InverseTransformVector(robotBody.velocity);
            currentVelocity.y = 0;
            if(TargetVelocity.magnitude > MaxSpeed)
            {
                TargetVelocity = TargetVelocity.normalized * MaxSpeed;
            }

            Vector3 force = velPID.UpdateVector3(TargetVelocity, currentVelocity, Time.fixedDeltaTime);
            force.y = 0;

            robotBody.AddForceAtPosition(force, robotBody.transform.position, ForceMode.Force);
            LastAppliedForce = force;
        }

    }
}
