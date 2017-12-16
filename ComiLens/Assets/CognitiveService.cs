using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.SpeechClient;
using BestHTTP;
using BestHTTP.WebSocket;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
namespace Assets
{
    public class CognitiveService : MonoBehaviour
    {
        private Subject<Message> _subject;
        public IObservable<Message> MessageObservable { get { return _subject; } }

        private const string LanguageJp = "ja-JP";
        private const string LanguageEn= "en-US";
        private const string ConversaationEndpoint = "wss://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=";
        private const string TokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        
        private const string SubscrpitionHeaderKey = "Ocp-Apim-Subscription-Key";
        private const string ContentTypeHeaderKey = "Content-type";
        private const string ContentTypeHeaderValue = "audio/wav; codec=audio/pcm; samplerate=16000";
        private const string AuthorizationHeaderKey = "Authorization";
        private const string AuthorizationHeaderValuePrefix = "Bearer ";

        private const string TransferEncodingHeaderKey = "Transfer-Encoding";
        private const string TransferEncodingHeaderValue = "chunked";


        private string _token;
        private WebSocket _webSocket;

        public bool IsConnected { get; private set; }

        private void ConnectWebSocket()
        {
            _webSocket = new WebSocket(new Uri(ConversaationEndpoint + LanguageJp));

            _webSocket.InternalRequest.SetHeader("Authorization", _token);
            _webSocket.InternalRequest.SetHeader("Upgrade", "websocket");
            _webSocket.InternalRequest.SetHeader("Connection", "Upgrade");
            _webSocket.InternalRequest.SetHeader("Origin", "https://speech.platform.bing.com");
            _webSocket.InternalRequest.SetHeader("Sec-WebSocket-Key", "dGhlIHNhbXBsZSBub25jZQ==");
            _webSocket.InternalRequest.SetHeader("Sec-WebSocket-Version", "13");
            _webSocket.InternalRequest.SetHeader("Origin", "https://speech.platform.bing.com");
            _webSocket.OnErrorDesc += (s, e) =>
            {
                Debug.Log("WebSocket OnError!");
            };
            _webSocket.OnClosed += (s, i, m) =>
            {
                Debug.Log("WebSocket OnClosed!");
            };
            _webSocket.OnMessage += (s, m) =>
            {
                try
                {
                    var parser = new PayloadParser();
                    var payload = parser.Parse(m);
                    var message = payload.GetMessage();

                    Debug.Log("WebSocket Message "+ payload.Path);
                    if (payload.Path == "speech.phrase")
                    {
                        Debug.Log("WebSocket Message Status" + payload.Content);

                        if (string.IsNullOrEmpty(message.DisplayText) == false)
                        {
                            Debug.Log("WebSocket OnMessageText!" + message.DisplayText);
                            _subject.OnNext(message);
                        }

                    }
                }
                catch (Exception e)
                {
                }
                
            };
            _webSocket.OnOpen += (s) =>
            {
                IsConnected = true;
                Debug.Log("WebSocket Open!");
            };
            _webSocket.OnError += (s, e) =>
            {
                IsConnected = false;
                Debug.Log("WebSocket Error!");
                string errorMsg = string.Empty;
                if (_webSocket.InternalRequest.Response != null)
                    errorMsg = string.Format("Status Code from Server: {0} and Message: {1}",
                        _webSocket.InternalRequest.Response.StatusCode,
                        _webSocket.InternalRequest.Response.Message);
                Debug.Log("An error occured: " + errorMsg);
            };
            _webSocket.Open();

        }

        public void Connect(string subscriptionKey)
        {
            Debug.Log(string.Format("Cognitive Request: {0}", subscriptionKey));
            StartCoroutine(RequestToken(subscriptionKey));
        }

        public IEnumerator RequestToken(string subscriptionKey)
        {
            var request = UnityWebRequest.Post(TokenEndpoint, "");
            request.SetRequestHeader(SubscrpitionHeaderKey, subscriptionKey);
            yield return request.Send();
            _token = request.downloadHandler.text;

            Debug.Log(string.Format("Cognitive Token: {0}", _token));
            ConnectWebSocket();
        }
        void Start()
        {
            _subject = new Subject<Message>();

            var service = GetComponent<CognitiveService>();
            service.Connect("");
        }

        private bool _isSendHeaader = false;

        public void Send(IEnumerable<byte> bytes)
        {
            if (_isSendHeaader)
            {
                _webSocket.Send(bytes.ToArray());
                return;
            }
            _isSendHeaader = true;
            IsConnected = false;

            var requestid = Guid.NewGuid().ToString("N");
            var outputBuilder = new StringBuilder();
            outputBuilder.Append("path:audio" + Environment.NewLine);
            outputBuilder.Append("x-requestid:" + requestid  + Environment.NewLine);
            outputBuilder.Append("x-timestamp:" +DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK")+ Environment.NewLine);
            outputBuilder.Append("content-type:audio/x-wav" + Environment.NewLine);

            var headerBytes = Encoding.ASCII.GetBytes(outputBuilder.ToString());
            //var headerbuffer = new ArraySegment<byte>(headerBytes, 0, headerBytes.Length);
            //var str = "0x" + (headerBytes.Length).ToString("X");
            var headerHeadBytes = BitConverter.GetBytes((UInt16)headerBytes.Length);
            var isBigEndian = !BitConverter.IsLittleEndian;
            var headerHead = !isBigEndian ? new byte[] { headerHeadBytes[1], headerHeadBytes[0] } : new byte[] { headerHeadBytes[0], headerHeadBytes[1] };

            //var length = Math.Min(4096 * 2 - headerBytes.Length - 8, currentChunk.AllBytes.Length - cursor); //8bytes for the chunk header

            //var chunkHeader = Encoding.ASCII.GetBytes("data").Concat(BitConverter.GetBytes((UInt32)length)).ToArray();

            //byte[] dataArray = new byte[length];
            //Array.Copy(currentChunk.AllBytes, cursor, dataArray, 0, length);

            //cursor += length;

            var arr = headerHead.Concat(headerBytes).Concat(bytes).ToArray();
            //var data = new List<byte>(GetWaveHeader());
            //data.AddRange(bytes);

            _webSocket.Send(arr);
        }

        /// <summary>
        ///     Create a RIFF Wave Header for PCM 16bit 16kHz Mono
        /// </summary>
        /// <returns></returns>
        private byte[] GetWaveHeader()
        {
            var extraSize = 0;
            var blockAlign = (short)(1 * (16 / 8));
            var averageBytesPerSecond = 16000 * blockAlign;

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream, Encoding.UTF8);
                writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(0);
                writer.Write(Encoding.UTF8.GetBytes("WAVE"));
                writer.Write(Encoding.UTF8.GetBytes("fmt "));
                writer.Write(18 + extraSize);
                writer.Write((short)1);
                writer.Write(1);
                writer.Write(16000);
                writer.Write(averageBytesPerSecond);
                writer.Write(blockAlign);
                writer.Write(16);
                writer.Write((short)extraSize);

                writer.Write(Encoding.UTF8.GetBytes("data"));
                writer.Write(0);

                stream.Position = 0;
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

    }
}
