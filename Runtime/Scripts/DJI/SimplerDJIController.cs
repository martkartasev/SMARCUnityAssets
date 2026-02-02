using Force;
using Smarc.GenericControllers;
using UnityEngine;
using VehicleComponents.Actuators;

namespace dji
{

    public enum DroneFlightState
    {
        Idle,
        TakingOff,
        Flying,
        Landing
    }

    /// <summary>
    /// This controller bridges between the DJI interface and a simple force-based set of controllers (smarc/generic controllers)
    /// </summary>
    [RequireComponent(typeof(AltitudeController))]
    [RequireComponent(typeof(AttitudeController))]
    [RequireComponent(typeof(HorizontalController))]
    public class SimplerDJIController : MonoBehaviour
    {
        [Header("Settings")]
        public bool StartInAir = false;
        public bool GotControl = true;

        [Header("Propellers")]
        public Propeller frontLeftPropeller;
        public Propeller frontRightPropeller;
        public Propeller backLeftPropeller;
        public Propeller backRightPropeller;
        public float FloatRPM = 1000f;

        float takeOffAltitude = 1.5f; // what the real thing does is 1.5m
        float homeAltitude; // altitude at which the drone took off
        public DroneFlightState flightState = DroneFlightState.Idle;

        AltitudeController altCtrl;
        AttitudeController attCtrl;
        HorizontalController horizCtrl;
        
        Vector3 commandedVelocity = Vector3.zero;
        float commandedYawRate = 0f;

        MixedBody robotBody;

        void Awake()
        {
            altCtrl = GetComponent<AltitudeController>();
            altCtrl.ControlMode = AltitudeControlMode.VerticalVelocity;
            altCtrl.TargetVelocity = 0f;

            attCtrl = GetComponent<AttitudeController>();
            attCtrl.YawControlMode = YawControlMode.YawRate;
            attCtrl.TargetYawRate = 0f;
            attCtrl.TiltMode = TiltMode.ReactToAcceleration;

            horizCtrl = GetComponent<HorizontalController>();
            horizCtrl.ControlMode = HorizontalControlMode.Velocity;
            horizCtrl.TargetVelocity = Vector3.zero;

            robotBody = new MixedBody(altCtrl.RobotAB, altCtrl.RobotRB);
            homeAltitude = robotBody.position.y;

            Ignition(StartInAir);
            if (StartInAir)
            {
                flightState = DroneFlightState.Flying;
                altCtrl.ControlMode = AltitudeControlMode.AbsoluteAltitude;
                altCtrl.TargetAltitude = homeAltitude;
                GotControl = true;
            }
        }


        void FixedUpdate()
        {
            if (!GotControl) return;

            switch(flightState)
            {
                case DroneFlightState.TakingOff:
                    altCtrl.ControlMode = AltitudeControlMode.AbsoluteAltitude;
                    altCtrl.TargetAltitude = homeAltitude + takeOffAltitude;
                    if (robotBody.position.y >= altCtrl.TargetAltitude - altCtrl.AltitudeTolerance)
                    {
                        flightState = DroneFlightState.Flying;
                        Debug.Log("Takeoff complete, now flying");
                    }
                    break;

                case DroneFlightState.Landing:
                    altCtrl.ControlMode = AltitudeControlMode.AbsoluteAltitude;
                    altCtrl.TargetAltitude = homeAltitude;
                    if (robotBody.position.y <= altCtrl.TargetAltitude + altCtrl.AltitudeTolerance)
                    {
                        flightState = DroneFlightState.Idle;
                        Debug.Log("Landing complete, now idle");
                        Ignition(false);
                    }
                    break;

                case DroneFlightState.Flying:
                    horizCtrl.ControlMode = HorizontalControlMode.Velocity;
                    horizCtrl.TargetVelocity = commandedVelocity;

                    altCtrl.ControlMode = AltitudeControlMode.VerticalVelocity;
                    altCtrl.TargetVelocity = commandedVelocity.y;

                    attCtrl.YawControlMode = YawControlMode.YawRate;
                    attCtrl.TargetYawRate = commandedYawRate;
                    break;
                case DroneFlightState.Idle:
                default:
                    // do nothing
                    break;
            }
        }

        public bool TakeOff()
        {
            if (!GotControl) 
            {
                Debug.Log("Cannot take off, do not have control");
                return false;
            }

            if (flightState != DroneFlightState.Idle)
            {
                Debug.Log("Cannot take off, drone not idle");
                return false;
            }

            Debug.Log("DJI Taking off");
            flightState = DroneFlightState.TakingOff;
            Ignition(true);
            homeAltitude = robotBody.position.y;
            return true;
        }

        public bool Land()
        {
            if (!GotControl) 
            {
                Debug.Log("Cannot land, do not have control");
                return false;
            }

            if (flightState != DroneFlightState.Flying)
            {
                Debug.Log("Cannot land, drone not flying");
                return false;
            }

            Debug.Log("DJI Landing");
            flightState = DroneFlightState.Landing;
            return true;
        }

        public bool TakeControl()
        {
            if (GotControl)
            {
                Debug.Log("Already have control");
                return true;
            }
            Debug.Log("DJI Taking control");
            GotControl = true;
            return true;
        }

        public bool ReleaseControl()
        {
            if (!GotControl)
            {
                Debug.Log("Do not have control to release");
                return false;
            }
            Debug.Log("DJI Releasing control");
            GotControl = false;
            return true;
        }

        void Ignition(bool on)
        {
            if (!GotControl)
            {
                Debug.Log("Cannot change ignition state, do not have control");
                return;
            }

            frontLeftPropeller.SetRpm(on? FloatRPM : 0f);
            frontRightPropeller.SetRpm(on? FloatRPM : 0f);
            backLeftPropeller.SetRpm(on? FloatRPM : 0f);
            backRightPropeller.SetRpm(on? FloatRPM : 0f);
            altCtrl.enabled = on;
            altCtrl.CompensateGravity = on;
            attCtrl.enabled = on;
            horizCtrl.enabled = on;
        }

        public void CommandFLUYawRate(float forward, float left, float up, float yawRate)
        {
            if (!GotControl)
            {
                Debug.Log("Cannot command drone, do not have control");
                return;
            }
            // Unity is RUF, do the mapping here.
            commandedVelocity = new Vector3(-left, up, forward);
            commandedYawRate = -yawRate;
        }
    }
}