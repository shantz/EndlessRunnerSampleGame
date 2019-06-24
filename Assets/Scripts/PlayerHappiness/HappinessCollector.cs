using System.Collections.Generic;
using System.Text;
using PlayerHappiness.Sensors;
using UnityEngine;

namespace PlayerHappiness
{
    public static class HappinessCollector
    {
        static bool m_Initialized;
        static List<ISensor> m_Sensors = new List<ISensor>();
        static List<CollectorContext> m_Contexts = new List<CollectorContext>();

        public static void Initialize()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;

                m_Sensors = new List<ISensor>();
                m_Contexts = new List<CollectorContext>();

                RegisterSensor(new GyroscopeSensor(0.5f));
                RegisterSensor(new TouchSensor());
            }
        }
        
        private static void RegisterSensor(ISensor sensor)
        {
            m_Sensors.Add(sensor);
        }

        public static void Start()
        {
            m_Contexts.Clear();

            float startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < m_Sensors.Count; i++)
            {
                m_Contexts.Add(new CollectorContext(startTime));
                m_Sensors[i].SetContext(m_Contexts[i]);
                
                m_Sensors[i].Start();
            }
        }

        public static void Stop(Dictionary<string, string> metadata)
        {
            for (var i = 0; i < m_Sensors.Count; i++)
            {
                m_Sensors[i].Stop();
            }
            
            Debug.Log(ToJSON());
        }

        public static string ToJSON()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{");
            
            for (var i = 0; i < m_Sensors.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(",");
                }

                var sensor = m_Sensors[i];
                var context = m_Contexts[i];

                builder.AppendFormat("\"{0}\":", sensor.GetType().Name);

                builder.Append("{");
                builder.Append("\"frames\":");
                builder.Append("[");
                for (int j = 0; j < context.Frames.Count; j++)
                {
                    var frame = context.Frames[i];
                
                    if (i != 0)
                    {
                        builder.Append(",");
                    }
                
                    builder.Append("{");
                    builder.AppendFormat("\"timestamp\":\"{0}\"", frame.timestamp);
                    WriteValues(builder, frame.floats);
                    WriteValues(builder, frame.vector2s);
                    WriteValues(builder, frame.vector3s);
                    WriteValues(builder, frame.quaternions);
                    builder.Append("}");
                }
                builder.Append("]");
            
                builder.Append("\"data\":");
                builder.Append("}");
            }
            
            builder.Append("}");
            
            return builder.ToString();
        }

        public static void Upload()
        {
            
        }
        
        static void WriteValues<T>(StringBuilder builder, List<FrameData<T>> datas)
        {
            foreach (var data in datas)
            {
                builder.AppendFormat("\"{0}\":\"{1}\"", data.name, data.value);
            }
        }
    }
}

