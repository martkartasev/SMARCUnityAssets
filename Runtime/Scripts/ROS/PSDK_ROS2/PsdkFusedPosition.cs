using Unity.Robotics.Core;
using UnityEngine;
using RosMessageTypes.PsdkInterfaces;
using ROS.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;



namespace M350.PSDK_ROS2
{
    [AddComponentMenu("Smarc/PSDK_ROS/PsdkFusedPos")]
    [RequireComponent(typeof(PsdkHomePosition))]
    public class PsdkFusedPos : ROSPublisher<PositionFusedMsg>
    {
        Transform base_link;
        PsdkHomePosition homePosition;

        protected override void InitPublisher()
        {
            base.InitPublisher();
            if (!GetBaseLink(out base_link))
            {
                Debug.LogError("No base_link found for PsdkFusedPosition. Make sure there is a link with the 'base_link' name under the robot root.");
                enabled = false;
                return;
            }
            ROSMsg.header.frame_id = "psdk_map_enu";
            homePosition = GetComponent<PsdkHomePosition>();
        }

        protected override void UpdateMessage(){
            // because the position is fused in the ENU frame, we need to make sure that
            // the position we are reading is in the ENU frame as well. 
            // We need the position of base wrt to home, but without the rotation of home 
            // (because in unity the base object==home could be rotated, so we cant just use localPosition of base_link)
            var ROSPosition = (base_link.position - homePosition.UnityHomePosition).To<ENU>();

            ROSMsg.position.x = ROSPosition.x;
            ROSMsg.position.y = ROSPosition.y;
            ROSMsg.position.z = ROSPosition.z;
            
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}