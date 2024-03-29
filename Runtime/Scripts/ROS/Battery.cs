using UnityEngine;
using RosMessageTypes.Sensor;

namespace DefaultNamespace
{
    public class Battery : Sensor<BatteryStateMsg>
    {
        [Header("Battery")]
        public float dischargePercentPerMinute = 1;
        public float currentPercent = 95f;

        public override bool UpdateSensor(double deltaTime)
        {
           currentPercent -= (float) ((deltaTime/60) * dischargePercentPerMinute);
           if(currentPercent < 0f) currentPercent = 0f;
           ros_msg.voltage = 12.5f;
           ros_msg.percentage = currentPercent;
           return true;
        }
    }
}
