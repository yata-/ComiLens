using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicComponent : MonoBehaviour
{
 //   private float _averageAmplitude;
 //   public float InputGain = 1;
 //   public MicStream.StreamCategory StreamType = MicStream.StreamCategory.HIGH_QUALITY_VOICE;
 //   private static void CheckForErrorOnCall(int returnCode)
 //   {
 //       MicStream.CheckForErrorOnCall(returnCode);
 //   }

 //   private void Awake()
 //   {
 //       CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)StreamType, AudioSettings.outputSampleRate));
 //   }

 //   void Update()
 //   {
 //       if (Input.GetKeyDown(KeyCode.W))
 //       {
 //           CheckForErrorOnCall(MicStream.MicStartStream(false, false));
 //           CheckForErrorOnCall(MicStream.MicSetGain(InputGain));
 //       }
 //       else if (Input.GetKeyDown(KeyCode.S))
 //       {
 //           CheckForErrorOnCall(MicStream.MicStopStream());

 //       }
 //   }
 //   // Use this for initialization
 //   void Start () {
		
	//}

 //   private void OnAudioFilterRead(float[] buffer, int numChannels)
 //   {
 //       // this is where we call into the DLL and let it fill our audio buffer for us
 //       CheckForErrorOnCall(MicStream.MicGetFrame(buffer, buffer.Length, numChannels));

 //       float sumOfValues = 0;

 //       // figure out the average amplitude from this new data
 //       for (int i = 0; i < buffer.Length; i++)
 //       {
 //           if (float.IsNaN(buffer[i]))
 //           {
 //               buffer[i] = 0;
 //           }

 //           buffer[i] = Mathf.Clamp(buffer[i], -1f, 1f);
 //           sumOfValues += Mathf.Clamp01(Mathf.Abs(buffer[i]));
 //       }
 //       _averageAmplitude = sumOfValues / buffer.Length;
 //   }
}
