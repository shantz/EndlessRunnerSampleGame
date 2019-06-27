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
	    const int MaxAttempts = 10;
        static bool m_Initialized;
        
        static List<ISensor> m_Sensors = new List<ISensor>();
        
        static List<FastCollectorContext> m_Contexts = new List<FastCollectorContext>();
        static Dictionary<string, string> m_Urls = new Dictionary<string, string>();
        static List<CustomYieldInstruction> m_Promisess = new List<CustomYieldInstruction>();
        
        public static float m_StartTime;
        public static float m_EndTime;

        public static bool isReady = true;

        public static GameObject s_Object;

        public static void Initialize()
        {
	        if (!m_Initialized)
            {
                m_Initialized = true;
                
                s_Object = new GameObject("Debug UI");
                s_Object.AddComponent<DebugUI>();
                GameObject.DontDestroyOnLoad(s_Object);

                m_Sensors = new List<ISensor>();
                m_Contexts = new List<FastCollectorContext>();
                m_Urls = new Dictionary<string, string>();
                
                RegisterSensor(new GyroscopeSensor(0.5f));
                RegisterSensor(new TouchSensor());
                RegisterSensor(new HeartbeatSensor());
                RegisterSensor(new GameSensor());
                RegisterSensor(new CameraSensor());
                RegisterSensor(new BrainSensor());
                RegisterSensor(new ScreenSensor());
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
            m_Promisess.Clear();

            m_StartTime = Time.realtimeSinceStartup;

            for (var i = 0; i < m_Sensors.Count; i++)
            {
                m_Contexts.Add(new FastCollectorContext(m_StartTime, m_Sensors[i].useFrames, m_Sensors[i].projectedValues));
                m_Sensors[i].SetContext(m_Contexts[i]);
                
                m_Sensors[i].Start();
            }
        }

        public static void Stop(Dictionary<string, string> metadata)
        {
	        isReady = false;
	        
	        m_EndTime =  Time.realtimeSinceStartup;
	        m_Promisess.Clear();
            for (var i = 0; i < m_Sensors.Count; i++)
            {
	            m_Promisess.Add((m_Sensors[i].Stop()));
            }
            
            #if !UNITY_EDITOR
	        CoroutineHandler.StartStaticCoroutine(UploadAll());
	        #endif
        }

        public static string ToJSON()
        {
	        StringBuilder builder = new StringBuilder();

            builder.Append("{");

            int writtenNodes = 0;
            
            for (var i = 0; i < m_Sensors.Count; i++)
            {
	            if (!m_Sensors[i].useFrames)
	            {
		            continue;
	            }
	            
                if (writtenNodes != 0)
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
                    WriteValues(builder, context.floats, frame.startFloats, frame.lengthFloats);
                    WriteValues(builder, context.ints, frame.startInts, frame.lengthInts);
                    WriteValues(builder, context.vector2s, frame.startVector2s, frame.lengthVector2s);
                    WriteValues(builder, context.vector3s, frame.startVector3s, frame.lengthVector3s);
                    WriteValues(builder, context.quaternions, frame.startQuaternions, frame.lengthQuaternions);
                    WriteValues(builder, context.strings, frame.startStrings, frame.lengthStrings);
                    builder.Append("}");
                }
                builder.Append("]");

                writtenNodes++;
            }

            builder.Append("}");
            
            return builder.ToString();
        }
        
        public static string ToSessionJSON()
        {
	        StringBuilder builder = new StringBuilder();

            builder.Append("{");

            foreach (var url in m_Urls)
            {
                builder.AppendFormat("\"{0}\":\"{1}\",",url.Key, url.Value);
            }
            
            builder.AppendFormat("\"length\":{0}", (int)Math.Round((m_EndTime - m_StartTime) * 1000));
            
            builder.Append("}");
            
            return builder.ToString();
        }
        
        static void WriteValues<T>(StringBuilder builder, List<FrameData<T>> datas, int starts, int length)
        {
	        for (int j = starts; j < starts + length; j++)
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
		        else
		        {
			        throw new Exception();
		        }
	        }
        }

        static IEnumerator UploadAll()
        {
	        yield return null;

            foreach (var customYieldInstruction in m_Promisess)
            {
	            yield return customYieldInstruction;
            }

            for (var i = 0; i < m_Sensors.Count; i++)
            {
	            m_Sensors[i].SetContext(null);
            }

            int k = 0;
            foreach (var collectorContext in m_Contexts)
            {
	            /*
	            if (collectorContext.Frames != null)
	            {
		            Debug.LogFormat("Context for {0}: floats - {1}, strings - {2}, vector2 - {3}, vector3 - {4}, quat - {5}, frames - {6}", m_Sensors[k].name
			            , collectorContext.floats.Count
			            , collectorContext.strings.Count
			            , collectorContext.vector2s.Count
			            , collectorContext.vector3s.Count
			            , collectorContext.quaternions.Count
			            , collectorContext.Frames.Count);

		            Debug.LogFormat("Capacity for {0}: floats - {1}, strings - {2}, vector2 - {3}, vector3 - {4}, quat - {5}, frames - {6}", m_Sensors[k].name
			            , collectorContext.floats.Capacity
			            , collectorContext.strings.Capacity
			            , collectorContext.vector2s.Capacity
			            , collectorContext.vector3s.Capacity
			            , collectorContext.quaternions.Capacity
			            , collectorContext.Frames.Capacity);
	            }
	            */

	            foreach (var file in collectorContext.MediaFile)
	            {
		            yield return UploadFile(file);
	            }

	            k++;
            }
            
            string fileName = Application.persistentDataPath + "/response.json";
            Debug.LogFormat("Writing response to a file: {0}", fileName);
            File.WriteAllText(fileName, ToJSON());
            
            yield return UploadFile(new KeyValuePair<string, string>("sensorDataUrl", fileName));
            
            yield return UploadJson();

            isReady = true;
        }

		static IEnumerator UploadJson()
		{
			string fileName = Application.persistentDataPath + "/session.json";
			Debug.LogFormat("Writing session to a file: {0}", fileName);
			File.WriteAllText(fileName, ToSessionJSON());

			int attempt = 0;
			while (attempt < MaxAttempts)
			{
				Debug.LogFormat("Attempt #{0} to upload JSON file...", attempt);
				
				attempt++;

				using (UnityWebRequest webRequest = new UnityWebRequest("http://hw19-player-happiness-api.unityads.unity3d.com/api/sessions", "POST"))
				{
					webRequest.uploadHandler = new UploadHandlerFile(fileName);
					webRequest.downloadHandler = new DownloadHandlerBuffer();

					var operation = webRequest.SendWebRequest();

					while (!operation.isDone) {
                        DebugUI.ProgressText = String.Format("JSON Upload {0}%", (int)(100 * webRequest.uploadProgress));
                        yield return null;
                    }
					DebugUI.ProgressText = null;

					if (webRequest.isHttpError || webRequest.isNetworkError)
					{
						Debug.LogErrorFormat("Failed to send JSON to server: {0}, {1}", webRequest.error, webRequest.responseCode);
						Debug.LogError(webRequest.downloadHandler.text);
					}
					else
					{
						break;
					}
				}
			}
		}

		static IEnumerator UploadFile(KeyValuePair<string, string> file)
		{
			int attempt = 0;
			while (attempt < MaxAttempts)
			{
				attempt++;
				Debug.LogFormat("Attempt #{0} to upload file {1}...", attempt, file.Key);
				
				using (UnityWebRequest uploadMedia = new UnityWebRequest("http://hw19-player-happiness-api.unityads.unity3d.com/api/uploads", "POST"))
				{
					uploadMedia.uploadHandler = new UploadHandlerFile(file.Value);
					uploadMedia.downloadHandler = new DownloadHandlerBuffer();

					var operation = uploadMedia.SendWebRequest();

					while (!operation.isDone) {
						DebugUI.ProgressText = String.Format("{0} Upload {1}%", file.Key, (int)(100 * uploadMedia.uploadProgress));
                        yield return null;
                    }
                    DebugUI.ProgressText = null;

                    if (uploadMedia.isHttpError || uploadMedia.isNetworkError)
					{
						Debug.LogErrorFormat("Failed to send media {0} to server: {1}", file.Key, uploadMedia.error);
					}
					else
					{
						UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(uploadMedia.downloadHandler.text);
						m_Urls[file.Key] = uploadResponse.downloadUrl;
						break;
					}
				}
			}
		}
    }

    class CustomCertifcateHandler : CertificateHandler
    {
	    protected override bool ValidateCertificate(byte[] certificateData)
	    {
		    return true;
	    }
    }
}

