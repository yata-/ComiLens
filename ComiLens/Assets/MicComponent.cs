using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
#else
using System.Runtime.InteropServices.WindowsRuntime;
#endif 

using System.Text;
using Assets;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

// Micからのオーディオ取得
// 参考
// https://qiita.com/miyaura/items/6f2570fe0dc0a8b0b7f1
[RequireComponent(typeof(AudioSource))]
public class MicComponent : MonoBehaviour
{
    private const int SamplingRate = 48000;
    private const int ApiSamplingRate = 16000;

    private float _timeElapsed;

    private CognitiveService _service;

    private List<short> _samplingData = new List<short>();

    // MicStreamに対してフィールドの変数を渡すとアプリが落ちる。。。
    //public bool KeepAllData = false;
    //public float InputGain = 1;
    //public MicStream.StreamCategory StreamType = MicStream.StreamCategory.HIGH_QUALITY_VOICE;

    private static void CheckForErrorOnCall(int returnCode)
    {
        MicStream.CheckForErrorOnCall(returnCode);
    }

    private void Awake()
    {
        var setting = AudioSettings.outputSampleRate;
        var category =  MicStream.StreamCategory.HIGH_QUALITY_VOICE;
        CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)category, setting));
        _service = this.GetComponent<CognitiveService>();
    }

    void Update()
    {
        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= 3)
        {
            if (this._service.IsConnected)
            {
                Debug.Log("[MicComponent]Send " + DateTime.Now.ToString());
                this._service.Send(ConvertBytes(_samplingData.ToArray()));
            }
            _samplingData.Clear();

            _timeElapsed = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckForErrorOnCall(MicStream.MicStopStream());
            WriteAudioData();
        }
    }

    void Start()
    {
        CheckForErrorOnCall(MicStream.MicStartStream(false, false));
        CheckForErrorOnCall(MicStream.MicSetGain(1));
        // MicStream.MicStartStreamのpreviewOnDeviceが動いてない？ので、直接0にしておく
        gameObject.GetComponent<AudioSource>().volume = 0; 
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
        CheckForErrorOnCall(MicStream.MicGetFrame(buffer, buffer.Length, numChannels));
        lock (this)
        {
            var convBuf = Resampling(buffer, numChannels);
            _samplingData.AddRange(convBuf);
        }
    }

    private static short[] Resampling(float[] buffer, int numChannels)
    {
        var reduction = SamplingRate / ApiSamplingRate * numChannels;
        var convBufSize = buffer.Length / reduction;
        if (buffer.Length % reduction > 0)
        {
            convBufSize++;
        }
        var convBuf = new short[convBufSize];
        var count = 0;
        float ave = 0;
        while (count < convBufSize - 1)
        {
            ave = 0;
            for (var i = 0; i < reduction; i++)
            {
                ave += buffer[count * reduction + i];
            }
            // 20fどこからきた？
            ave = ave / reduction * 20f;
            convBuf[count] = FloatToInt16(ave);
            count++;
        }
        ave = 0;
        for (var i = count * reduction; i < buffer.Length; i++)
        {
            ave += buffer[i];
        }
        ave = ave / (buffer.Length + count * reduction + 1);
        convBuf[count] = FloatToInt16(ave);
        return convBuf;
    }

    private IEnumerable<byte> ConvertBytes(short[] sampleData)
    {
        foreach (var s in sampleData)
        {
            var bytes = BitConverter.GetBytes(s);
            yield return bytes[0];
            yield return bytes[1];
        }
    }
    private void WriteAudioData()
    {
        Debug.Log("WriteAudioData:");
        var fileName = string.Format("StreamingDataHolo{0}.wav", DateTime.Now.ToString("yyyyMMMMdd_hhmmss"));
        var headerSize = 46;
        short extraSize = 0;

        short toBitsPerSample = 16;
        short toChannels = 1;
        int toSampleRate = ApiSamplingRate;
        var blockAlign = (short)(toChannels * (toBitsPerSample / 8));
        var averageBytesPerSecond = toSampleRate * blockAlign;

        var samplingDataSize = _samplingData.Count;
        var sampingDataByteSize = samplingDataSize * blockAlign;

#if UNITY_EDITOR
        using (var file = new FileStream(@"C:\Users\guide\Desktop\comirens\sound\" + fileName, FileMode.Create))
        {
#else
        var task = System.Threading.Tasks.Task.Run(async () =>
        {
            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using (var outputStrm = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
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
                    var dat = BitConverter.GetBytes(_samplingData[i]);
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
