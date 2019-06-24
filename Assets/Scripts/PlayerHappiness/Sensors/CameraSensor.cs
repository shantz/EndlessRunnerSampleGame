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
    public class CameraSensor : ISensor
    {
        ICollectorContext m_Context;

		[Header("Recording")]
		public int videoWidth = 1280;
		public int videoHeight = 720;

		private MP4Recorder videoRecorder;
		private IClock recordingClock;
		public string recordedFilePath;
        private DeviceCamera deviceCamera;

        private Texture cameraTexture;

        private bool recording;

        public string name => "facecam";
        public CameraSensor()
        {
			recordingClock = new RealtimeClock();

			videoHeight = Screen.height;
			videoWidth = Screen.width;
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
            deviceCamera.StartPreview(OnStart, OnFrame);
			// Start recording
			recordingClock = new RealtimeClock();
			videoRecorder = new MP4Recorder(
				videoWidth,
				videoHeight,
				30,
				0,
				0,
				FileLocationCB
			);
            recording = true;
        }


		private void FileLocationCB(string path)
		{
			Debug.Log("Saved recording to: " + path);
			recordedFilePath = path;
			m_Context.SetMetdataFile("CameraFile", path);
#if UNITY_EDITOR
			PlaybackRecording();
#endif
		}
		public void Stop()
		{
            // Stop recording
            recording = false;
            videoRecorder.Dispose();
		}
        void OnStart (Texture preview) {
        // Display the camera preview
            cameraTexture =  preview;
        }
        void OnFrame () {
            if (!recording) {
                return;
            }

            var frame = videoRecorder.AcquireFrame();
            Graphics.Blit(cameraTexture, frame);
            videoRecorder.CommitFrame(frame, recordingClock.Timestamp);
        }

    }
}
