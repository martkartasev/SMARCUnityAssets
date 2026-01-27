using UnityEngine;
using Force;
using DefaultNamespace;
using System;

namespace dji
{
    public class ReactiveTilt : MonoBehaviour
    {
        public ArticulationBody RobotAB;
        public Rigidbody RobotRB;
        MixedBody robotBody;

        public HorizontalController horizontalController;

        // We want to tilt the robot based on its horizontal acceleration
        // to fake the tilt a drone would use to generate horizontal thrust
        public float MaxTiltAngle = 20f; // maximum tilt angle in degrees
        public float ExpectedMaxForce = 10f; // expected maximum horizontal force
        public float Kp = 1.0f; // proportional gain for tilt control



        void Start()
        {
            robotBody = new MixedBody(RobotAB, RobotRB);
        }

        void FixedUpdate()
        {
            Vector3 appliedForce = horizontalController.LastAppliedForce;
            appliedForce.y = 0; // should already be, but just to be safe
            float mag = appliedForce.magnitude;
            var currentAngVel = robotBody.angularVelocity;
            float targetTiltAngle = 0f;
            if (mag > 0.05f)
            {
                targetTiltAngle = mag / ExpectedMaxForce * MaxTiltAngle;
            }
        
            // keep the target angle within -180 to 180 range
            if (Math.Abs(targetTiltAngle) > 180f) targetTiltAngle -= Mathf.Sign(targetTiltAngle) * 360f;

            Vector3 tiltAxis = Vector3.Cross(Vector3.up, appliedForce.normalized);
            Quaternion targetRotation = Quaternion.AngleAxis(targetTiltAngle * Kp, tiltAxis);

            float diffX = Mathf.DeltaAngle(robotBody.transform.localEulerAngles.x, targetRotation.eulerAngles.x);
            float diffZ = Mathf.DeltaAngle(robotBody.transform.localEulerAngles.z, targetRotation.eulerAngles.z);

            // set the robotbody's velocity to match the target rotation
            float angVelX = diffX / Time.fixedDeltaTime;
            float angVelZ = diffZ / Time.fixedDeltaTime;

            // limit the angular velocity to avoid instability
            float maxAngVel = 20f; // degrees per second
            angVelX = Mathf.Clamp(angVelX, -maxAngVel, maxAngVel);
            angVelZ = Mathf.Clamp(angVelZ, -maxAngVel, maxAngVel);
            
            robotBody.angularVelocity = new Vector3(angVelX*Mathf.Deg2Rad, currentAngVel.y, angVelZ*Mathf.Deg2Rad);

            Debug.Log($"targetTiltAngle: {targetTiltAngle}, tiltAxis: {tiltAxis}, appliedForce: {appliedForce}, angVel: {robotBody.angularVelocity}");
        }
    }
}