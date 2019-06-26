using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PlayerHappiness.Sensors;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace PlayerHappiness
{
    class DebugUI : MonoBehaviour
    {
        GameObject Brain_Ping;
        GameObject Heart_Ping;
        GameObject Heart_Conencted;
        GameObject QR_Code;
        Texture2D QR_Texture;
        StringBuilder timestampBuffer = new StringBuilder(128);

        static BarcodeWriter writer;

        private static Color32[] Encode(string textForEncoding, int width, int height) {
            if (writer == null) {
                writer = new BarcodeWriter {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions {
                        Height = height,
                        Width = width
                    }
                };
            }
            return writer.Write(textForEncoding);
        }

        private void generateQR(string text) {
            var encoded = QR_Texture;
            var color32 = Encode(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
        }

        int lastBrainFrame;
        int lastHeartFrame;
        GameObject IP_Object;

        void Start()
        {
            GameObject myGO;
            Canvas myCanvas;

            // Canvas
            myGO = new GameObject();
            myGO.name = "TestCanvas";
            myGO.AddComponent<Canvas>();
            
            myGO.transform.SetParent(this.gameObject.transform);

            myCanvas = myGO.GetComponent<Canvas>();
            myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            myGO.AddComponent<CanvasScaler>();
            myGO.AddComponent<GraphicRaycaster>();
            
            Heart_Conencted = new GameObject();
            Heart_Conencted.transform.parent = myGO.transform;
            Heart_Conencted.name = "Heart_Conencted";
            
            Heart_Conencted.AddComponent<CanvasRenderer>();
            Image i = Heart_Conencted.AddComponent<Image>();
            i.color = Color.red;
            Heart_Conencted.transform.SetParent(myCanvas.transform, false);
            
            Heart_Conencted.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 0);
            
            Heart_Ping = new GameObject();
            Heart_Ping.transform.parent = myGO.transform;
            Heart_Ping.name = "Heart_Ping";
            
            Heart_Ping.AddComponent<CanvasRenderer>();
            i = Heart_Ping.AddComponent<Image>();
            i.color = Color.magenta;
            Heart_Ping.transform.SetParent(myCanvas.transform, false);
            
            Heart_Ping.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, 0);
            
            Brain_Ping = new GameObject();
            Brain_Ping.transform.parent = myGO.transform;
            Brain_Ping.name = "Brain_Ping";
            
            Brain_Ping.AddComponent<CanvasRenderer>();
            i = Brain_Ping.AddComponent<Image>();
            i.color = Color.yellow;
            Brain_Ping.transform.SetParent(myCanvas.transform, false);
            
            Brain_Ping.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            // QR_Code = new GameObject();
            // QR_Code.transform.parent = myGO.transform;
            // QR_Code.name = "QR_Code";

            // QR_Code.AddComponent<CanvasRenderer>();
            // var qrImage = QR_Code.AddComponent<RawImage>();
            // QR_Code.transform.SetParent(myCanvas.transform, false);

            // QR_Code.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            QR_Texture = new Texture2D(256, 256);
            // qrImage.texture = QR_Texture;
            QR_Code = GameObject.Find("QRImage");
            QR_Code.GetComponent<RawImage>().texture = QR_Texture;

            IP_Object = new GameObject();
            IP_Object.transform.parent = myGO.transform;
            IP_Object.name = "IP";
            
            IP_Object.AddComponent<CanvasRenderer>();
            Text t = IP_Object.AddComponent<Text>();
            i.color = Color.black;
            t.text = "IP: " + LocalIPAddress();
            t.fontSize = 40;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            IP_Object.transform.SetParent(myCanvas.transform, false);
            
            IP_Object.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            IP_Object.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);
        }

        void Update()
        {
            if (Time.frameCount % 60 == 0)
            {
#if !UNITY_EDITOR
                Brain_Ping.SetActive(lastBrainFrame == BrainSensor.currentFrame);
                Heart_Conencted.SetActive(!HeartbeatSensor.connected);
                Heart_Ping.SetActive(HeartbeatSensor.currentFrame == lastHeartFrame);
                
                IP_Object.SetActive(Brain_Ping.activeSelf || Heart_Conencted.activeSelf || Heart_Ping.activeSelf);

                lastBrainFrame = BrainSensor.currentFrame;
                lastHeartFrame = HeartbeatSensor.currentFrame;
                #else
                Brain_Ping.SetActive(false);
                Heart_Conencted.SetActive(false);
                Heart_Ping.SetActive(false);
                
                IP_Object.SetActive(false);

#endif
            }

            var deltaTime = Time.realtimeSinceStartup - PlayerHappiness.HappinessCollector.m_StartTime;

            if (deltaTime < 3)
            {
                timestampBuffer.Clear();
                timestampBuffer.AppendFormat("{0}", (int)Math.Round(deltaTime * 1000));
                generateQR(timestampBuffer.ToString());
                QR_Code.SetActive(true);
            }
            else
            {
                QR_Code.SetActive(false);
            }
        }
        
        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "0.0.0.0";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
