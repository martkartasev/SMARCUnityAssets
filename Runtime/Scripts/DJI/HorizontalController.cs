using UnityEngine;
using Force;
using DefaultNamespace;
using UnityEngine.InputSystem;


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

        [Header("Debug")]
        public bool EnableKeyboardControl = false;


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
            // check if the robot is upright enough to control horizontal movement
            var upDot = Vector3.Dot(robotBody.transform.up, Vector3.up);
            if (upDot < 0.5f)
            {
                Debug.Log($"Robot too tilted for horizontal control! upDot: {upDot}");
                return;
            }

            var currentSpeed = robotBody.localVelocity.magnitude;
            if (currentSpeed > MaxSpeed*1.1f)
            {
                Debug.Log($"Robot moving too fast for horizontal control! currentSpeed: {currentSpeed}");
                return;
            }

            if (ControlMode == HorizontalControlMode.UnityPosition)
            {
                Vector3 diff = TargetUnityPosition - robotBody.transform.position;
                if (diff.magnitude <= PositionTolerance) TargetVelocity = Vector3.zero;
                else TargetVelocity = diff.normalized * MaxSpeed;
            }
            TargetVelocity.y = 0;

            if(EnableKeyboardControl)
            {
                float inputZ = Keyboard.current.wKey.isPressed ? 1f : (Keyboard.current.sKey.isPressed ? -1f : 0f);
                float inputX = Keyboard.current.dKey.isPressed ? 1f : (Keyboard.current.aKey.isPressed ? -1f : 0f);
                Vector3 inputDir = new Vector3(inputX, 0, inputZ);
                if(inputDir.magnitude > 0f)
                {
                    inputDir.Normalize();
                    TargetVelocity = inputDir * MaxSpeed;
                }
                else
                {
                    TargetVelocity = Vector3.zero;
                }
            }
            VelocityControl();
        }

        void VelocityControl()
        {
            Vector3 currentVelocity = robotBody.localVelocity;
            currentVelocity.y = 0;
            if(TargetVelocity.magnitude > MaxSpeed)
            {
                TargetVelocity = TargetVelocity.normalized * MaxSpeed;
            }

            Vector3 force = velPID.UpdateVector3(TargetVelocity, currentVelocity, Time.fixedDeltaTime);
            force.y = 0;

            force = robotBody.transform.TransformVector(force);
            robotBody.AddForceAtPosition(force, robotBody.transform.position, ForceMode.Force);
            LastAppliedForce = force;
        }

    }
}
