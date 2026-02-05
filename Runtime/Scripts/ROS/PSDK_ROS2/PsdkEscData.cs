using Unity.Robotics.Core;
using RosMessageTypes.PsdkInterfaces;
using ROS.Core;
using UnityEngine;
using VehicleComponents.Actuators;



namespace M350.PSDK_ROS2
{
    [AddComponentMenu("Smarc/PSDK_ROS/PsdkEscData")]
    public class PsdkEscData : ROSPublisher<EscDataMsg>
    {

        public Propeller p0,p1,p2,p3;
        protected override void UpdateMessage() 
        {
            /*
            Makes a "dummyESC" that always publishes that all 4 props are at a speed of 4000. 
            Could be improved by tying to true prop speeds.
            It is necessary to publish these such that the captain knows that the drone is flying.
            */
            EscStatusIndividualMsg e0 = new EscStatusIndividualMsg();
            e0.speed = (short)p0.rpm;
            EscStatusIndividualMsg e1 = new EscStatusIndividualMsg();
            e1.speed = (short)p1.rpm;
            EscStatusIndividualMsg e2 = new EscStatusIndividualMsg();
            e2.speed = (short)p2.rpm;
            EscStatusIndividualMsg e3 = new EscStatusIndividualMsg();
            e3.speed = (short)p3.rpm;
            
            EscStatusIndividualMsg[] dummyESCs = new EscStatusIndividualMsg[4];
            dummyESCs[0] = e0;
            dummyESCs[1] = e1;
            dummyESCs[2] = e2;
            dummyESCs[3] = e3;

            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.esc = dummyESCs;
        }
    }
}