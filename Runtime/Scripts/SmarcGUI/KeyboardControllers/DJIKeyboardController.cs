using UnityEngine;
using UnityEngine.InputSystem;
using Smarc.GenericControllers;
using dji;

namespace SmarcGUI.KeyboardControllers
{
    [RequireComponent(typeof(AltitudeController))]
    [RequireComponent(typeof(AttitudeController))]
    [RequireComponent(typeof(HorizontalController))]
    [RequireComponent(typeof(DJIController))]
    public class DJIKeyboardController : KeyboardControllerBase
    {
        InputAction forwardAction, strafeAction, verticalAction, tvAction;
        AltitudeController altCtrl;
        AttitudeController attCtrl;
        HorizontalController horizCtrl;
        DJIController djiCtrl;

        void Awake()
        {
            forwardAction = InputSystem.actions.FindAction("Robot/Forward");
            strafeAction = InputSystem.actions.FindAction("Robot/Strafe");
            verticalAction = InputSystem.actions.FindAction("Robot/UpDown");
            tvAction = InputSystem.actions.FindAction("Robot/ThrustVector");
            
            altCtrl = GetComponent<AltitudeController>();
            attCtrl = GetComponent<AttitudeController>();
            horizCtrl = GetComponent<HorizontalController>();
            djiCtrl = GetComponent<DJIController>();
        }

        void OnEnable()
        {
            djiCtrl.ReleaseControl();
        }

        void OnDisable()
        {
            djiCtrl.TakeControl();
        }

        void Update()
        {
            var forwardValue = forwardAction.ReadValue<float>();
            var strafeValue = strafeAction.ReadValue<float>();
            var tvValue = tvAction.ReadValue<Vector2>();
            var yawValue = tvValue.x;
            var verticalValue = verticalAction.ReadValue<float>();

            switch (djiCtrl.flightState)
            {
                case DroneFlightState.Flying:
                    horizCtrl.ControlMode = HorizontalControlMode.Velocity;
                    horizCtrl.TargetVelocity = new Vector3(strafeValue, 0f, forwardValue).normalized * horizCtrl.MaxSpeed;

                    attCtrl.YawControlMode = YawControlMode.YawRate;
                    attCtrl.TargetYawRate = yawValue * attCtrl.DesiredYawRate;

                    altCtrl.ControlMode = AltitudeControlMode.VerticalVelocity;
                    float e = 0.01f;
                    if (verticalValue > e) altCtrl.TargetVelocity = altCtrl.AscentRate;
                    else if (verticalValue < -e) altCtrl.TargetVelocity = -altCtrl.DescentRate;
                    else altCtrl.TargetVelocity = 0f;

                    break;
                case DroneFlightState.Idle:
                case DroneFlightState.Landing:
                case DroneFlightState.TakingOff:
                default:
                    break;
            }
        }

        public override void OnReset()
        {
            horizCtrl.TargetVelocity = Vector3.zero;
            attCtrl.TargetYawRate = 0f;
            altCtrl.TargetVelocity = 0f;
        }

    }
}