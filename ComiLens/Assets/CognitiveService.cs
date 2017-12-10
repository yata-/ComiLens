using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BestHTTP.WebSocket;
using UnityEngine;
using UnityEngine.Networking;
namespace Assets
{
    public class CognitiveService : MonoBehaviour
    {
        private const string ConversaationEndpoint = "https://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US";
        private const string TokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

        private string _subscriptionKey;
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
            _webSocket = new WebSocket(new Uri(ConversaationEndpoint));
            _webSocket.InternalRequest.SetHeader(AuthorizationHeaderKey, AuthorizationHeaderValuePrefix+ _token);
            _webSocket.InternalRequest.SetHeader(ContentTypeHeaderKey, ContentTypeHeaderValue);
            _webSocket.InternalRequest.SetHeader(TransferEncodingHeaderKey, TransferEncodingHeaderValue);
            _webSocket.OnMessage += (s, m) =>
            {

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
            var service = GetComponent<CognitiveService>();
            service.Connect("");
        }

        public void Send(byte[] bytes)
        {
            _webSocket.Send(bytes);
        }
    }
}
