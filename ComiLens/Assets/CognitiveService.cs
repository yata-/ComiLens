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
        private Subject<Payload> _subject;
        public IObservable<Payload> MessageObservable { get { return _subject; } }

        private const string LanguageJp = "ja-JP";
        private const string LanguageEn= "en-US";
        private const string ConversaationEndpoint = "wss://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=";
        private const string TokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        
        private const string SubscrpitionHeaderKey = "Ocp-Apim-Subscription-Key";
        private const string UpgradeHeaderKey = "Upgrade";
        private const string AuthorizationHeaderKey = "Authorization";

        private const string TransferEncodingHeaderKey = "Transfer-Encoding";
        private const string TransferEncodingHeaderValue = "chunked";

        private string _token;
        private WebSocket _webSocket;

        public bool IsConnected { get; private set; }

        private void ConnectWebSocket()
        {
            _webSocket = new WebSocket(new Uri(ConversaationEndpoint + LanguageJp));

            _webSocket.InternalRequest.SetHeader(AuthorizationHeaderKey, _token);
            _webSocket.InternalRequest.SetHeader(UpgradeHeaderKey, "websocket");
            _webSocket.InternalRequest.SetHeader("Connection", "Upgrade");
            _webSocket.InternalRequest.SetHeader("Origin", "https://speech.platform.bing.com");
            _webSocket.InternalRequest.SetHeader("Sec-WebSocket-Key", "dGhlIHNhbXBsZSBub25jZQ==");
            _webSocket.InternalRequest.SetHeader("Sec-WebSocket-Version", "13");
            _webSocket.OnErrorDesc += (s, e) =>
            {
                Debug.Log("S OnError!");
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

                    Debug.Log("WebSocket Message "+ payload.Path);
                    _subject.OnNext(payload);
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

        private void Connect(string subscriptionKey)
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
            _subject = new Subject<Payload>();
            Connect("");
        }

        public void Send(IEnumerable<byte> bytes)
        {
            var headerBytes = new RequestHeader().ToBytes();
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
            _webSocket.Send(arr);
        }

        private class RequestHeader
        {
            private const string PathKeyValue = "path:audio";
            private const string TypeKeyValue = "content-type:audio/x-wav";

            private const string RequestKey = "x-requestid";
            private const string TimeStampKey = "x-timestamp";


            private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
            public DateTime RequestDate { get; private set; }
            public string Id { get; private set; }
            public RequestHeader()
            {
                Id = Guid.NewGuid().ToString("N");
                RequestDate = DateTime.UtcNow;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append(PathKeyValue + Environment.NewLine);
                sb.Append(string.Format("{0}:{1}{2}", RequestKey, Id , Environment.NewLine));
                sb.Append(string.Format("{0}:{1}{2}", TimeStampKey,  RequestDate.ToString(DateFormat) , Environment.NewLine));
                sb.Append(TypeKeyValue + Environment.NewLine);
                return sb.ToString();
            }

            public byte[] ToBytes()
            {
                return Encoding.ASCII.GetBytes(ToString());
            }

        }

    }
}
