using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using Force;
using RosMessageTypes.Nav;
using Scripts.ROS.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace ROS.Publishers.GroundTruth
{
    public class GT_Odom_Pub : ROSPublisher<OdometryMsg>
    {
        public CoordinateSpaceSelection targetSpace = CoordinateSpaceSelection.ENU;

        MixedBody body;

        protected override void InitPublisher()
        {
            if (GetBaseLink(out var base_link))
            {
                var base_link_ab = base_link.GetComponent<ArticulationBody>();
                var base_link_rb = base_link.GetComponent<Rigidbody>();
                body = new MixedBody(base_link_ab, base_link_rb);
                if (!body.isValid)
                {
                    Debug.LogError("Base link doesnt have a valid Rigidbody or ArticulationBody.");
                    enabled = false;
                    return;
                }

                GetRobotGO(out var robotGO);
                ROSMsg.header.frame_id = "unity_origin";
                ROSMsg.child_frame_id = $"{robotGO.name}/base_link";
            }
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.pose.pose.position = body.position.ToROS(targetSpace);
            ROSMsg.pose.pose.orientation = body.rotation.ToROS(targetSpace);

            var vel = body.transform.InverseTransformVector(body.velocity); 
            ROSMsg.twist.twist.linear = vel.To<FLU>();
            var anvel = body.transform.InverseTransformVector(body.angularVelocity);
            ROSMsg.twist.twist.angular = -anvel.To<FLU>();
        }
    }
}
