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
    public class ScreenSensor : ISensor
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

		public ScreenSensor()
        {
			recordingClock = new RealtimeClock();

			videoHeight = Screen.height;
			videoWidth = Screen.width;
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
			// no
			recordMicrophone = false;

			// Start recording
			recordingClock = new RealtimeClock();
			videoRecorder = new MP4Recorder(
				videoWidth,
				videoHeight,
				30,
				recordMicrophone ? AudioSettings.outputSampleRate : 0,
				recordMicrophone ? (int)AudioSettings.speakerMode : 0,
				FileLocationCB
			);
			// Create recording inputs
			cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);
        }


		private void FileLocationCB(string path)
		{
			Debug.Log("Saved recording to: " + path);
			recordedFilePath = path;
			m_Context.SetMetdataFile("VideoFile", path);
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

		public void Stop()
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
		}

		private void StopMicrophone()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			Microphone.End(null);
			microphoneSource.Stop();
#endif
		}

    }
}
