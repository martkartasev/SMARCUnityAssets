using Smarc.GenericControllers;
using UnityEngine;

namespace Evolo
{
    [RequireComponent(typeof(AltitudeController))]
    [RequireComponent(typeof(AttitudeController))]
    [RequireComponent(typeof(HorizontalController))]
    public class EvoloController : MonoBehaviour
    {
        [Header("Targets")]
        public float Speed;
        public float YawRate;
        public float Altitude = 1f;
        
        [Header("Limits")]
        public float MaxYawRate = 5f;
        public float MaxSpeed = 7f;
        public float MaxAltitude = 2f;

        AltitudeController altCtrl;
        AttitudeController attCtrl;
        HorizontalController horizCtrl;

        void Awake()
        {
            altCtrl = GetComponent<AltitudeController>();
            altCtrl.ControlMode = AltitudeControlMode.AltitudeFromWater;
            altCtrl.TargetAltitude = Altitude;

            attCtrl = GetComponent<AttitudeController>();
            attCtrl.YawControlMode = YawControlMode.YawRate;
            attCtrl.TargetYawRate = 0f;
            attCtrl.TiltMode = TiltMode.ReactToAcceleration;

            horizCtrl = GetComponent<HorizontalController>();
            horizCtrl.ControlMode = HorizontalControlMode.Velocity;
            horizCtrl.TargetVelocity = Vector3.zero;
        }

        void FixedUpdate()
        {
            Speed = Mathf.Clamp(Speed, -MaxSpeed, MaxSpeed);
            horizCtrl.TargetVelocity = new Vector3(0, 0, Mathf.Clamp(Speed, -MaxSpeed, MaxSpeed));
            YawRate = Mathf.Clamp(YawRate, -MaxYawRate, MaxYawRate);
            attCtrl.TargetYawRate = Mathf.Clamp(YawRate, -MaxYawRate, MaxYawRate);
            Altitude = Mathf.Clamp(Altitude, 0, MaxAltitude);
            altCtrl.TargetAltitude = Altitude;
        }
    }
}