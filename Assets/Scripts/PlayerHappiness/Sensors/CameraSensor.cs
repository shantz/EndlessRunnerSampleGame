using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatCam;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PlayerHappiness.Sensors
{
    public class CameraSensor : CustomYieldInstruction, ISensor
    {
        ICollectorContext m_Context;

		[Header("Recording")]
		public int videoWidth = 360;
		public int videoHeight = 640;

		private MP4Recorder videoRecorder;
		private IClock recordingClock;
		public string recordedFilePath;
        private DeviceCamera deviceCamera;

        private Texture cameraTexture;
        
        bool m_IsReady;

        public string name => "facecam";
        
        public CameraSensor()
        {
			recordingClock = new RealtimeClock();
			
            deviceCamera = DeviceCamera.FrontCamera;
        }

		public void PlaybackRecording()
		{
			// Playback the video
#if UNITY_EDITOR
			EditorUtility.OpenWithDefaultApp(recordedFilePath);
#elif UNITY_IOS
            Handheld.PlayFullScreenMovie("file://" + recordedFilePath);
#elif UNITY_ANDROID
            Handheld.PlayFullScreenMovie(recordedFilePath);
#endif
		}

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
	        m_IsReady = false;
	        
			// Start recording
			recordingClock = new RealtimeClock();
			videoRecorder = new MP4Recorder(
				videoWidth,
				videoHeight,
				15,
				0,
				0,
				FileLocationCB,
				"facecam.mp4",
				(int)(480 * 540 * 11.4f)
			);

			deviceCamera.Framerate = 15;
			deviceCamera.StartPreview(OnStart, OnFrame);
        }

        private void FileLocationCB(string path)
		{
			Debug.Log("Saved recording to: " + path);
			
			recordedFilePath = path;
			m_Context.SetMetdataFile("cameraVideoUrl", path);
			m_IsReady = true;
#if UNITY_EDITOR
			PlaybackRecording();
#endif
		}
        
		public CustomYieldInstruction Stop()
		{
			// Stop recording
			deviceCamera.StopPreview();
            videoRecorder.Dispose();
            return this;
		}
		
        void OnStart (Texture preview) {
        // Display the camera preview
            cameraTexture =  preview;
            GameObject fooImage = GameObject.Find("FooImage");
            if (fooImage != null) {
                fooImage.GetComponent<RawImage>().texture = preview;
            }
        }
        
        void OnFrame () {
	        var frame = videoRecorder.AcquireFrame();
            Graphics.Blit(cameraTexture, frame);
            videoRecorder.CommitFrame(frame, recordingClock.Timestamp);
        }

        public override bool keepWaiting => !m_IsReady;
    }
}
