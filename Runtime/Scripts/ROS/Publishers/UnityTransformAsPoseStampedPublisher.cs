using ROS.Core;
using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

namespace Scripts.ROS.Publishers
{
    public class UnityTransformAsPoseStampedPublisher : ROSPublisher<PoseStampedMsg>
    {
        public string parentFrameId = "";
        public Transform parentFrame;
        
        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.header.frame_id = parentFrameId;

            var pos = parentFrame.InverseTransformPoint(transform.position).To<NED>();
            var orient = (Quaternion.Inverse(parentFrame.transform.rotation) * transform.rotation).To<NED>();

            ROSMsg.pose.position.x = pos.x;
            ROSMsg.pose.position.x = pos.y;
            ROSMsg.pose.position.x = pos.z;

            ROSMsg.pose.orientation.x = orient.x;
            ROSMsg.pose.orientation.y = orient.y;
            ROSMsg.pose.orientation.z = orient.z;
            ROSMsg.pose.orientation.w = orient.w;
        }
    }
}