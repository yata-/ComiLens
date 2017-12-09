using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicComponent : MonoBehaviour
{
    private readonly List<short> samplingData = new List<short>();
    private float _averageAmplitude;
    public bool KeepAllData = false;
    public float InputGain = 1;
    public MicStream.StreamCategory StreamType = MicStream.StreamCategory.ROOM_CAPTURE;
    private static void CheckForErrorOnCall(int returnCode)
    {
        MicStream.CheckForErrorOnCall(returnCode);
    }

    private void Awake()
    {
        CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)StreamType, AudioSettings.outputSampleRate));
        CheckForErrorOnCall(MicStream.MicStartStream(KeepAllData, false));
        CheckForErrorOnCall(MicStream.MicSetGain(InputGain));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckForErrorOnCall(MicStream.MicStopStream());
            WriteAudioData();
        }
    }
    // Use this for initialization
    void Start()
    {
    }
    private void OnDestroy()
    {
        CheckForErrorOnCall(MicStream.MicDestroy());
    }
    private static short FloatToInt16(float value)
    {
        var f = value * short.MaxValue;
        if (f > short.MaxValue) f = short.MaxValue;
        if (f < short.MinValue) f = short.MinValue;
        return (short)f;
    }
    private void OnAudioFilterRead(float[] buffer, int numChannels)
    {
        // this is where we call into the DLL and let it fill our audio buffer for us
        CheckForErrorOnCall(MicStream.MicGetFrame(buffer, buffer.Length, numChannels));
        foreach (var f in buffer)
        {
            samplingData.Add(FloatToInt16(f));
        }
    }

    private void WriteAudioData()
    {
        var fileName = "StreamingDataHolo.wav";
        var headerSize = 46;
        short extraSize = 0;

        short toBitsPerSample = 16;
        short toChannels = 2;
        int toSampleRate = AudioSettings.outputSampleRate;
        var blockAlign = (short)(toChannels * (toBitsPerSample / 8));
        var averageBytesPerSecond = toSampleRate * blockAlign;

        var samplingDataSize = samplingData.Count;
        var sampingDataByteSize = samplingDataSize * blockAlign; //DataSize

#if UNITY_EDITOR
        using (var file = new FileStream(@"C:\Users\guide\Desktop\comirens" + fileName, FileMode.Create))
        {
#else
        task = Task.Run(async () =>
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var outputStrm = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
#endif

            var bytes = Encoding.UTF8.GetBytes("RIFF");
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(headerSize + sampingDataByteSize - 8);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = Encoding.UTF8.GetBytes("WAVE");
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = Encoding.UTF8.GetBytes("fmt ");
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(18);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes((short)1);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(toChannels);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(toSampleRate);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(averageBytesPerSecond);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(blockAlign);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(toBitsPerSample);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(extraSize);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = Encoding.UTF8.GetBytes("data");
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            
            bytes = BitConverter.GetBytes(sampingDataByteSize);
#if UNITY_EDITOR
            file.Write(bytes, 0, bytes.Length);
#else
            await outputStrm.WriteAsync(bytes.AsBuffer());
#endif            


            for (var i = 0; i < samplingDataSize; i++)
            {
                var dat = BitConverter.GetBytes(samplingData[i]);
#if UNITY_EDITOR
                file.Write(dat, 0, dat.Length);
#else
                await outputStrm.WriteAsync(dat.AsBuffer());
#endif
            }
        }
#if !UNITY_EDITOR
        });
        task.Wait();
#endif
    }
}
