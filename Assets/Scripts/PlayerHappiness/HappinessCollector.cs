using System.Collections;
using System.Collections.Generic;
using System.Text;
using PlayerHappiness.Sensors;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayerHappiness
{
    public static class HappinessCollector
    {
        static bool m_Initialized;
        static List<ISensor> m_Sensors = new List<ISensor>();
        static List<CollectorContext> m_Contexts = new List<CollectorContext>();
        static Dictionary<string, string> m_Urls = new Dictionary<string, string>();

        public static void Initialize()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;

                m_Sensors = new List<ISensor>();
                m_Contexts = new List<CollectorContext>();
                m_Urls = new Dictionary<string, string>();
                
                RegisterSensor(new GyroscopeSensor(0.5f));
                RegisterSensor(new FakeMedia());
            }
        }
        
        private static void RegisterSensor(ISensor sensor)
        {
            m_Sensors.Add(sensor);
        }

        public static void Start()
        {
            m_Contexts.Clear();
            m_Urls.Clear();

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

            CoroutineHandler.StartStaticCoroutine(UploadAll());
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
                    var frame = context.Frames[j];
                
                    if (j != 0)
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
                builder.Append("}");
            }

            foreach (var url in m_Urls)
            {
                builder.AppendFormat(",\"{0}\":\"{1}\"",url.Key, url.Value);
            }
            
            builder.Append("}");
            
            return builder.ToString();
        }

        static void WriteValues<T>(StringBuilder builder, List<FrameData<T>> datas)
        {
            for (int j = 0; j < datas.Count; j++)
            {
                builder.AppendFormat(",\"{0}\":\"{1}\"", datas[j].name, datas[j].value);
            }
        }
        
        static IEnumerator UploadAll()
        {
            yield return null;

            foreach (var collectorContext in m_Contexts)
            {
                foreach (var bytes in collectorContext.Media)
                {
                    UnityWebRequest uploadMedia = new UnityWebRequest("http://34.98.89.204/api/uploads", "POST");

                    uploadMedia.uploadHandler = new UploadHandlerRaw(bytes.Value);

                    yield return uploadMedia.SendWebRequest();

                    if (uploadMedia.isHttpError || uploadMedia.isNetworkError)
                    {
                        Debug.LogErrorFormat("Failed to send media {0} to server: {1}", bytes.Key,  uploadMedia.error);
                    }
                    else
                    {
                        m_Urls[bytes.Key] = uploadMedia.downloadHandler.text;
                    }
                }
            }
            
            UnityWebRequest webRequest = UnityWebRequest.Post("http://34.98.89.204/api/sessions", ToJSON());

            yield return webRequest.SendWebRequest();

            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogErrorFormat("Failed to send JSON to server: {0}", webRequest.error);
            }
        }
    }
}

