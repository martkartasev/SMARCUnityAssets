using UnityEngine;
using RosMessageTypes.Nav;
using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using SensorIMU = VehicleComponents.Sensors.IMU;
using ROS.Core;
using Scripts.ROS.Core;


namespace ROS.Publishers
{
    [RequireComponent(typeof(SensorIMU))]
    public class OdomFromIMU_Pub : ROSSensorPublisher<OdometryMsg, SensorIMU>
    {
        public CoordinateSpaceSelection targetSpace = CoordinateSpaceSelection.ENU;

        [Header("Debug")] public Vector3 ROSPosition;

        public OdometryMsg GetRosMsg()
        {
            return ROSMsg;
        }

        protected override void InitPublisher()
        {
            ROSMsg.header.frame_id = "unity_origin";
            ROSMsg.child_frame_id = $"{robot_name}/{DataSource.linkName}";
            ROSPosition = Vector3.zero;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            
            ROSMsg.pose.pose.orientation = DataSource.orientation.ToROS(targetSpace);
            ROSMsg.pose.pose.position = DataSource.transform.position.ToROS(targetSpace);
            
            ROSPosition.x = (float)ROSMsg.pose.pose.position.x;
            ROSPosition.y = (float)ROSMsg.pose.pose.position.y;
            ROSPosition.z = (float)ROSMsg.pose.pose.position.z;

            ROSMsg.twist.twist.linear = DataSource.localVelocity.To<FLU>();
            ROSMsg.twist.twist.angular = DataSource.angularVelocity.To<FLU>();
        }
    }
}