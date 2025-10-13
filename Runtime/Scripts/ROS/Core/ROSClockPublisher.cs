using System;
using UnityEngine;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Rosgraph;
using ROS.Core;

namespace Unity.Robotics.Core
{
    public class ROSClockPublisher : ROSBehaviour
    {
        [SerializeField]
        Clock.ClockMode m_ClockMode;

        [SerializeField, HideInInspector]
        Clock.ClockMode m_LastSetClockMode;
        
        [SerializeField] 
        double m_PublishRateHz = 100f;

        double m_LastPublishTimeSeconds;

        TimeMsg clockMsg;

        double PublishPeriodSeconds => 1.0f / m_PublishRateHz;

        bool ShouldPublishMessage => Clock.FrameStartTimeInSeconds - PublishPeriodSeconds > m_LastPublishTimeSeconds;

        bool registered = false;

        void OnValidate()
        {
            // var clocks = FindObjectsOfType<ROSClockPublisher>();
            var clocks = FindObjectsByType<ROSClockPublisher>(FindObjectsSortMode.None);
            if (clocks.Length > 1)
            {
                Debug.LogWarning("Found too many clock publishers in the scene, there should only be one!");
            }

            if (Application.isPlaying && m_LastSetClockMode != m_ClockMode)
            {
                Debug.LogWarning("Can't change ClockMode during simulation! Setting it back...");
                m_ClockMode = m_LastSetClockMode;
            }
            
            SetClockMode(m_ClockMode);
        }

        void SetClockMode(Clock.ClockMode mode)
        {
            Clock.Mode = mode;
            m_LastSetClockMode = mode;
        }


        protected override void StartROS()
        {
            SetClockMode(m_ClockMode);
            if (!registered)
            {
                rosCon.RegisterPublisher<ClockMsg>("/clock");
                registered = true;
            }
            clockMsg = new TimeMsg();
        }


        void Update()
        {
            if (!ShouldPublishMessage) return;

            var publishTime = Clock.time;
            clockMsg.sec = (int)publishTime;
            clockMsg.nanosec = (uint)((publishTime - Math.Floor(publishTime)) * Clock.k_NanoSecondsInSeconds);
            m_LastPublishTimeSeconds = publishTime;
            rosCon.Publish("/clock", clockMsg);
        }
    }
}