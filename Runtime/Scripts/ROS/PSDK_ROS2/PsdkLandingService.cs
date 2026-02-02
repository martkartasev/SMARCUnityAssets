using RosMessageTypes.Std;
using dji;

using ROS.Core;
using UnityEngine;

namespace M350.PSDK_ROS2
{
    [AddComponentMenu("Smarc/PSDK_ROS/PsdkLandingService")]
    public class PsdkLandingService : ROSBehaviour
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
                rosCon.ImplementService<TriggerRequest, TriggerResponse>(topic, _landing_callback);
                registered = true;
            }
        }

        private TriggerResponse _landing_callback(TriggerRequest request){
            TriggerResponse response = new TriggerResponse();
            if (controller == null)
            {
                controller = GetComponentInParent<IDJIController>();
                if (controller == null)
                {
                    Debug.Log("Controller not found");
                    response.success = false;
                    return response;
                }
            }

            response.success = controller.Land();
            return response;
        }
    }
}