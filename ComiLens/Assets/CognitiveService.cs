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
        private const string RequestSubscrpitionHeaderKey = "Ocp-Apim-Subscription-Key";
        private const string RequestContentTypeHeaderKey = "Content-type";
        private const string RequestContentTypeHeaderValue = "audio/wav; codec=audio/pcm; samplerate=16000";
        private string _token;
        private WebSocket _webSocket;

        public bool IsConnected;

        public void StartConnection()
        {
            _webSocket = new WebSocket(new Uri(ConversaationEndpoint));
            _webSocket.InternalRequest.SetHeader(RequestSubscrpitionHeaderKey, _token);
            _webSocket.InternalRequest.SetHeader(RequestContentTypeHeaderKey, RequestContentTypeHeaderValue);
            _webSocket.OnMessage += (s, m) =>
            {

            };
            _webSocket.OnOpen += (s) =>
            {
                Debug.Log("WebSocket Open!");
            };
        }

        public void Connect(string subscriptionKey)
        {
            Debug.Log(string.Format("Cognitive Request: {0}", subscriptionKey));
            StartCoroutine(RequestToken(subscriptionKey));
        }

        public IEnumerator RequestToken(string subscriptionKey)
        {
            var request = UnityWebRequest.Post(TokenEndpoint, "");
            request.SetRequestHeader(RequestSubscrpitionHeaderKey, subscriptionKey);
            yield return request.Send();
            _token = request.downloadHandler.text;

            Debug.Log(string.Format("Cognitive Token: {0}", _token));
        }
        void Start()
        {
            var service = GetComponent<CognitiveService>();
            service.Connect("");
            
        }
    }
}
