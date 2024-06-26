//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Smarc
{
    [Serializable]
    public class ControllerStatusMsg : Message
    {
        public const string k_RosMessageName = "smarc_msgs/ControllerStatus";
        public override string RosMessageName => k_RosMessageName;

        public const byte CONTROL_STATUS_NOT_CONTROLLING = 0;
        public const byte CONTROL_STATUS_CONTROLLING = 1;
        public const byte CONTROL_STATUS_ERROR = 2;
        public byte control_status;
        //  name of service to turn on and off controller
        public string service_name;
        //  optional diagnostics message
        public string diagnostics_message;

        public ControllerStatusMsg()
        {
            this.control_status = 0;
            this.service_name = "";
            this.diagnostics_message = "";
        }

        public ControllerStatusMsg(byte control_status, string service_name, string diagnostics_message)
        {
            this.control_status = control_status;
            this.service_name = service_name;
            this.diagnostics_message = diagnostics_message;
        }

        public static ControllerStatusMsg Deserialize(MessageDeserializer deserializer) => new ControllerStatusMsg(deserializer);

        private ControllerStatusMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.control_status);
            deserializer.Read(out this.service_name);
            deserializer.Read(out this.diagnostics_message);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.control_status);
            serializer.Write(this.service_name);
            serializer.Write(this.diagnostics_message);
        }

        public override string ToString()
        {
            return "ControllerStatusMsg: " +
            "\ncontrol_status: " + control_status.ToString() +
            "\nservice_name: " + service_name.ToString() +
            "\ndiagnostics_message: " + diagnostics_message.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
