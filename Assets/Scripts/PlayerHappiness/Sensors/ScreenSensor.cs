using System.Collections;
using UnityEngine;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PlayerHappiness.Sensors
{
	class ScreenSensor : CustomYieldInstruction, ISensor
    {
        ICollectorContext m_Context;

		[Header("Recording")]
		public int videoWidth = 1280;
		public int videoHeight = 720;

 		[Header("Microphone")]
 		public bool recordMicrophone = false;
 		public AudioSource microphoneSource;

		private MP4Recorder videoRecorder;
		private IClock recordingClock;
		private CameraInput cameraInput;
		private AudioInput audioInput;
		public string recordedFilePath;
		bool m_IsReady;

		public ScreenSensor()
        {
			recordingClock = new RealtimeClock();

			videoHeight = Screen.height / 2;
			videoWidth = Screen.width / 2;
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
		
		public string name => "screen";
		public bool useFrames => false;
		public int[] projectedValues => new[] { /* f */ 0,  /* i */ 0,  /* s */ 0,  /* v2 */ 0,  /* v3 */ 0,  /* q */ 0 };

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
	        m_IsReady = false;
			// no
			recordMicrophone = false;

			// Start recording
			recordingClock = new RealtimeClock();
			videoRecorder = new MP4Recorder(
				videoWidth,
				videoHeight,
				15,
				recordMicrophone ? AudioSettings.outputSampleRate : 0,
				recordMicrophone ? (int)AudioSettings.speakerMode : 0,
				FileLocationCB,
				"gamevideo.mp4",
				(int)(480 * 540 * 11.4f)
			);
            // Create recording inputs
            cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);
        }


		private void FileLocationCB(string path)
		{
			Debug.Log("Saved recording to: " + path);
			recordedFilePath = path;
			m_Context.SetMetdataFile("screenVideoUrl", path);
			m_IsReady = true;
#if UNITY_EDITOR
			PlaybackRecording();
#endif
		}
		private void StartMicrophone()
		{
#if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
			// Create a microphone clip
			microphoneSource.clip = Microphone.Start(null, true, 60, 48000);
			while (Microphone.GetPosition(null) <= 0) ;
			// Play through audio source
			microphoneSource.timeSamples = Microphone.GetPosition(null);
			microphoneSource.loop = true;
			microphoneSource.Play();
#endif
		}

		public CustomYieldInstruction Stop()
		{
			// Stop the recording inputs
			if (recordMicrophone)
			{
				StopMicrophone();
				audioInput.Dispose();
			}
			cameraInput.Dispose();
			// Stop recording
			videoRecorder.Dispose();

			return this;
		}

		private void StopMicrophone()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			Microphone.End(null);
			microphoneSource.Stop();
#endif
		}

		public override bool keepWaiting => !m_IsReady;
    }
}
