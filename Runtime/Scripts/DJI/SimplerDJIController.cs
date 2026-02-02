using Force;
using Smarc.GenericControllers;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public class SimplerDJIController : MonoBehaviour, IDJIController
    {
        [Header("Settings")]
        public bool StartInAir = false;

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

        MixedBody robotBody;


        void Awake()
        {
            altCtrl = GetComponent<AltitudeController>();
            altCtrl.ControlMode = AltitudeControlMode.AbsoluteAltitude;

            attCtrl = GetComponent<AttitudeController>();
            horizCtrl = GetComponent<HorizontalController>();

            robotBody = new MixedBody(altCtrl.RobotAB, altCtrl.RobotRB);
            homeAltitude = robotBody.position.y;

            Ignition(StartInAir);
            if (StartInAir)
            {
                flightState = DroneFlightState.Flying;
                altCtrl.ControlMode = AltitudeControlMode.AbsoluteAltitude;
                altCtrl.TargetAltitude = homeAltitude;
            }
        }


        void FixedUpdate()
        {
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
                case DroneFlightState.Idle:
                default:
                    // do nothing
                    break;
            }
        }

        public bool TakeOff()
        {
            if (flightState != DroneFlightState.Idle)
            {
                Debug.Log("Cannot take off, drone not idle");
                return false;
            }
            Debug.Log("Taking off");
            flightState = DroneFlightState.TakingOff;
            Ignition(true);
            return true;
        }

        public bool Land()
        {
            if (flightState != DroneFlightState.Flying)
            {
                Debug.Log("Cannot land, drone not flying");
                return false;
            }
            Debug.Log("Landing");
            flightState = DroneFlightState.Landing;
            return true;
        }

        void Ignition(bool on)
        {
            frontLeftPropeller.SetRpm(on? FloatRPM : 0f);
            frontRightPropeller.SetRpm(on? FloatRPM : 0f);
            backLeftPropeller.SetRpm(on? FloatRPM : 0f);
            backRightPropeller.SetRpm(on? FloatRPM : 0f);
            altCtrl.enabled = on;
            altCtrl.CompensateGravity = on;
            attCtrl.enabled = on;
            horizCtrl.enabled = on;
        }
    }
}