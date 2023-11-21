using System;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;
using Unity.Robotics.UrdfImporter;

namespace DefaultNamespace
{
    public class SAMArticulationModel : MonoBehaviour, IForceModel, ISAMControl
    {
        public ArticulationBody baseLink;
        public ArticulationBody nozzleLink;
        public ArticulationBody thrustLink;
        public ArticulationBody propeller1;
        public ArticulationBody propeller2;
        public double lcg { get; set; }
        public double vbs { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        public void Start()
        {
            baseLink.enabled = true;
            nozzleLink.enabled = true;
            thrustLink.enabled = true;
            propeller1.enabled = true;
            propeller2.enabled = true;
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            this.rpm1 = rpm1;
            this.rpm2 = rpm2;
        }

        public void SetRudderAngle(float dr)
        {
            d_rudder = dr;
        }

        public void SetElevatorAngle(float de)
        {
            d_aileron = de;
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg;
        }

        public void SetWaterPump(double vbs)
        {
            this.vbs = vbs;
        }

        private void FixedUpdate()
        {
            var r1 = rpm1 / 2000; // Normalize with assumption that max == 2000
            var r2 = rpm2 / 2000;

            Debug.Log(baseLink.inertiaTensor + ":::" + nozzleLink.inertiaTensor);

            nozzleLink.SetDriveTarget(ArticulationDriveAxis.Y, -Mathf.Rad2Deg * d_rudder);
            nozzleLink.SetDriveTarget(ArticulationDriveAxis.Z, Mathf.Rad2Deg * d_aileron);

            propeller1.SetDriveTargetVelocity(ArticulationDriveAxis.X, (float)(r1 * 2000));
            propeller2.SetDriveTargetVelocity(ArticulationDriveAxis.X, (float)(-r2 * 2000));

            thrustLink.AddForce(thrustLink.transform.forward * (float)r1 * 5);
            thrustLink.AddForce(thrustLink.transform.forward * (float)r2 * 5);
        }


        public Vector3 GetTorqueDamping()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetForceDamping()
        {
            throw new System.NotImplementedException();
        }
    }
}
