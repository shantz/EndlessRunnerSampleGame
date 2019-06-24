using System;
using System.Collections;
using System.Collections.Generic;
using PlayerHappiness;
using UnityEngine;

namespace DefaultNamespace
{
    public class SampleCollector : MonoBehaviour
    {
        CoroutineExecutor m_CoroutineExecutor;
        
        void Start()
        {
            m_CoroutineExecutor = new CoroutineExecutor();
            HappinessCollector.RegisterSensor(new GyroSensor(m_CoroutineExecutor, 0.5f));
        }

        void OnEnable()
        {
            HappinessCollector.Start();
        }

        void OnDisable()
        {
            HappinessCollector.Stop(new Dictionary<string, string>());
        }
    }
    
    public class GyroSensor : ISensor
    {
        readonly ICoroutineExecutor m_CoroutineExecutor;
        ICollectorContext m_Context;
        
        public GyroSensor(ICoroutineExecutor coroutineExecutor, float updateInterval)
        {
            m_CoroutineExecutor = coroutineExecutor;
            Input.gyro.updateInterval = updateInterval;
        }
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        IEnumerator CollectFrame()
        {
            while (Input.gyro.enabled)
            {
                using (var frame = m_Context.DoFrame())
                {
                    frame.Write("rotationRate", Input.gyro.rotationRate);
                    frame.Write("gravity", Input.gyro.gravity);
                    frame.Write("userAcceleration", Input.gyro.userAcceleration);
                    frame.Write("rotationRateUnbiased", Input.gyro.rotationRateUnbiased);
                    frame.Write("attitude", Input.gyro.attitude);
                }
                
                yield return new WaitForSeconds(Input.gyro.updateInterval);
            }
        }

        public void Start()
        {
            Input.gyro.enabled = true;
            m_CoroutineExecutor.StartCoroutine(CollectFrame());
        }

        public void Stop()
        {
            Input.gyro.enabled = false;
        }
    }
}
