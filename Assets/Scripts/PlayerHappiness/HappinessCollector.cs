using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        static List<CustomYieldInstruction> m_Promisess = new List<CustomYieldInstruction>();
        static float m_StartTime;
        static float m_EndTime;

        public static void Initialize()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;

                m_Sensors = new List<ISensor>();
                m_Contexts = new List<CollectorContext>();
                m_Urls = new Dictionary<string, string>();
                
                RegisterSensor(new GyroscopeSensor(0.5f));
                RegisterSensor(new TouchSensor());
                RegisterSensor(new HeartbeatSensor());
                RegisterSensor(new GameSensor());
                RegisterSensor(new CameraSensor());
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

            m_StartTime = Time.realtimeSinceStartup;

            for (var i = 0; i < m_Sensors.Count; i++)
            {
                m_Contexts.Add(new CollectorContext(m_StartTime));
                m_Sensors[i].SetContext(m_Contexts[i]);
                
                m_Sensors[i].Start();
            }
        }

        public static void Stop(Dictionary<string, string> metadata)
        {
	        m_EndTime =  Time.realtimeSinceStartup;
	        m_Promisess.Clear();
            for (var i = 0; i < m_Sensors.Count; i++)
            {
	            m_Promisess.Add((m_Sensors[i].Stop()));
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

                builder.AppendFormat("\"{0}\":", sensor.name);
                builder.Append("[");
                for (int j = 0; j < context.Frames.Count; j++)
                {
                    var frame = context.Frames[j];
                
                    if (j != 0)
                    {
                        builder.Append(",");
                    }
                
                    builder.Append("{");
                    builder.AppendFormat("\"ts\":{0}", (int)Math.Round(frame.timestamp * 1000));
                    WriteValues(builder, frame.floats);
					WriteValues(builder, frame.ints);
					WriteValues(builder, frame.vector2s);
					WriteValues(builder, frame.vector3s);
                    WriteValues(builder, frame.quaternions);
                    WriteValues(builder, frame.strings);
                    builder.Append("}");
                }
                builder.Append("]");
            }

            foreach (var url in m_Urls)
            {
                builder.AppendFormat(",\"{0}\":\"{1}\"",url.Key, url.Value);
            }
            
            builder.AppendFormat(",\"Length\":{0}", (int)Math.Round((m_EndTime - m_StartTime)) * 1000);
            
            builder.Append("}");
            
            return builder.ToString();
        }

        static void WriteValues<T>(StringBuilder builder, List<FrameData<T>> datas)
        {
	        for (int j = 0; j < datas.Count; j++)
            {
	            if (typeof(T).Equals(typeof(float)) || typeof(T).Equals(typeof(int)))
	            {
		            builder.AppendFormat(",\"{0}\": {1}", datas[j].name, datas[j].value);
	            }
	            else if(typeof(T).Equals(typeof(string)))
	            {
					builder.AppendFormat(",\"{0}\": \"{1}\"", datas[j].name, datas[j].value);
	            }
				else if (typeof(T).Equals(typeof(Vector2)))
	            {
		            Vector2 value = (Vector2)(object)datas[j].value;
		            builder.AppendFormat(",\"{0}\": [{1},{2}]", datas[j].name,value.x, value.y);
	            }
	            else if (typeof(T).Equals(typeof(Vector3)))
	            {
		            Vector3 value = (Vector3)(object)datas[j].value;
		            builder.AppendFormat(",\"{0}\": [{1},{2},{3}]", datas[j].name,value.x, value.y, value.z);
	            }
	            else if (typeof(T).Equals(typeof(Quaternion)))
	            {
		            Quaternion value = (Quaternion)(object)datas[j].value;
		            builder.AppendFormat(",\"{0}\": [{1},{2},{3},{4}]", datas[j].name,value.x, value.y, value.z, value.w);
	            }
            }
        }

		static void WriteValue(StringBuilder builder, string name, float value)
		{
			builder.AppendFormat(",\"{0}\":{1}", name, value);
		}

		static void WriteValue(StringBuilder builder, string name, Vector2 value)
		{
			builder.AppendFormat(",\"{0}\":", name);
			builder.Append("{");
			builder.AppendFormat("\"x\":{0},", value.x);
			builder.AppendFormat("\"y\":{0}", value.y);
			builder.Append("}");
		}

		static void WriteValue(StringBuilder builder, string name, Vector3 value)
		{
			builder.AppendFormat(",\"{0}\":", name);
			builder.Append("{");
			builder.AppendFormat("\"x\":{0},", value.x);
			builder.AppendFormat("\"y\":{0},", value.y);
			builder.AppendFormat("\"z\":{0}", value.z);
			builder.Append("}");
		}

		static void WriteValue(StringBuilder builder, string name, Quaternion value)
		{
			builder.AppendFormat(",\"{0}\":", name);
			builder.Append("{");
			builder.AppendFormat("\"w\":{0},", value.w);
			builder.AppendFormat("\"x\":{0},", value.x);
			builder.AppendFormat("\"y\":{0},", value.y);
			builder.AppendFormat("\"z\":{0}", value.z);
			builder.Append("}");
		}

		static void WriteValue(StringBuilder builder, string name, string value)
		{
			builder.AppendFormat(",\"{0}\":\"{1}\"", name, value);
		}

		static void WriteValue<T>(StringBuilder builder, string name, T value)
		{
			builder.AppendFormat(",\"{0}\":{1}", name, value);
		}

		static IEnumerator UploadAll()
        {
            yield return null;

            foreach (var customYieldInstruction in m_Promisess)
            {
	            yield return customYieldInstruction;
            }

            foreach (var collectorContext in m_Contexts)
            {
                foreach (var bytes in collectorContext.Media)
                {
                    UnityWebRequest uploadMedia = new UnityWebRequest("https://hw19-player-happiness-api.unityads.unity3d.com/api/uploads", "POST");

                    uploadMedia.uploadHandler = new UploadHandlerRaw(bytes.Value);
					uploadMedia.downloadHandler = new DownloadHandlerBuffer();

					yield return uploadMedia.SendWebRequest();

                    if (uploadMedia.isHttpError || uploadMedia.isNetworkError)
                    {
                        Debug.LogErrorFormat("Failed to send media {0} to server: {1}", bytes.Key,  uploadMedia.error);
                    }
                    else
                    {
						UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(uploadMedia.downloadHandler.text);
						m_Urls[bytes.Key] = uploadResponse.downloadUrl;

					}
                }

                foreach (var file in collectorContext.MediaFile)
                {
                    UnityWebRequest uploadMedia = new UnityWebRequest("https://hw19-player-happiness-api.unityads.unity3d.com/api/uploads", "POST");

                    uploadMedia.uploadHandler = new UploadHandlerFile(file.Value);
                    uploadMedia.downloadHandler = new DownloadHandlerBuffer();

                    yield return uploadMedia.SendWebRequest();

                    if (uploadMedia.isHttpError || uploadMedia.isNetworkError)
                    {
                        Debug.LogErrorFormat("Failed to send media {0} to server: {1}", file.Key,  uploadMedia.error);
                    }
                    else
                    {
	                    UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(uploadMedia.downloadHandler.text);
	                    m_Urls[file.Key] = uploadResponse.downloadUrl;
                    }
                }
            }

            string fileName = Application.persistentDataPath + "/response.json";
            Debug.LogFormat("Writing response to a file: {0}", fileName);
            File.WriteAllText( fileName, ToJSON());
            
            UnityWebRequest webRequest = new UnityWebRequest("https://hw19-player-happiness-api.unityads.unity3d.com/api/sessions", "POST");
            webRequest.uploadHandler = new UploadHandlerFile(fileName);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            yield return webRequest.SendWebRequest();

            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogErrorFormat("Failed to send JSON to server: {0}, {1}", webRequest.error, webRequest.responseCode);
                Debug.LogError(webRequest.downloadHandler.text);
            }
        }
    }
}

