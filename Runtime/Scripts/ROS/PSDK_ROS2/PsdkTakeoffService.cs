using UnityEngine;
using RosMessageTypes.Std;
using dji;

using ROS.Core;


namespace M350.PSDK_ROS2
{
    [AddComponentMenu("Smarc/PSDK_ROS/PsdkTakeoffService")]
    public class PsdkTakeoffService : ROSBehaviour
    {
        bool registered = false;
        IDJIController controller = null;


        protected override void StartROS()
        {
            if(controller == null){
                controller = GetComponentInParent<IDJIController>();
            }
            if (!registered)
            {
                rosCon.ImplementService<TriggerRequest, TriggerResponse>(topic, _takeoff_callback);
                registered = true;
            }
        }

        private TriggerResponse _takeoff_callback(TriggerRequest request)
        {
            TriggerResponse response = new TriggerResponse();
            Debug.Log("Take off service running");
            if (controller == null)
            {
                Debug.Log("Finding Controller Component");
                controller = GetComponentInParent<IDJIController>();
                if (controller == null)
                {
                    Debug.Log("Controller not found");
                    response.success = false;
                    return response;
                }
            }

            response.success = controller.TakeOff();
            return response;
        }

    }
}