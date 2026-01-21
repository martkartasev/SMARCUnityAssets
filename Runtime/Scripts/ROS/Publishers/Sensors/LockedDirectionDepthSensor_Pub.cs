using UnityEngine;
using RosMessageTypes.Std;
using SensorDepth = VehicleComponents.Sensors.LockedDirectionDepthSensor;
using ROS.Core;

namespace ROS.Publishers
{
    [AddComponentMenu("Smarc/ROS/Publishers/Sensors/LockedDirectionDepthSensor_Pub")]
    [RequireComponent(typeof(SensorDepth))]
    class LockedDirectionDepthSensor_Pub : ROSSensorPublisher<Float32Msg, SensorDepth>
    { 

        protected override void UpdateMessage()
        {
            ROSMsg.data =  DataSource.depth ;
        }
    }
}
