//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Sam
{
    [Serializable]
    public class CircuitStatusMsg : Message
    {
        public const string k_RosMessageName = "sam_msgs/CircuitStatus";
        public override string RosMessageName => k_RosMessageName;

        // 
        //  From uavcan CiruitStatus msg: Generic electrical circuit info.
        // 
        public const byte ERROR_FLAG_OVERVOLTAGE = 1;
        public const byte ERROR_FLAG_UNDERVOLTAGE = 2;
        public const byte ERROR_FLAG_OVERCURRENT = 4;
        public const byte ERROR_FLAG_UNDERCURRENT = 8;
        public byte error_flags;
        public ushort circuit_id;
        public float voltage;
        public float current;

        public CircuitStatusMsg()
        {
            this.error_flags = 0;
            this.circuit_id = 0;
            this.voltage = 0.0f;
            this.current = 0.0f;
        }

        public CircuitStatusMsg(byte error_flags, ushort circuit_id, float voltage, float current)
        {
            this.error_flags = error_flags;
            this.circuit_id = circuit_id;
            this.voltage = voltage;
            this.current = current;
        }

        public static CircuitStatusMsg Deserialize(MessageDeserializer deserializer) => new CircuitStatusMsg(deserializer);

        private CircuitStatusMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.error_flags);
            deserializer.Read(out this.circuit_id);
            deserializer.Read(out this.voltage);
            deserializer.Read(out this.current);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.error_flags);
            serializer.Write(this.circuit_id);
            serializer.Write(this.voltage);
            serializer.Write(this.current);
        }

        public override string ToString()
        {
            return "CircuitStatusMsg: " +
            "\nerror_flags: " + error_flags.ToString() +
            "\ncircuit_id: " + circuit_id.ToString() +
            "\nvoltage: " + voltage.ToString() +
            "\ncurrent: " + current.ToString();
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