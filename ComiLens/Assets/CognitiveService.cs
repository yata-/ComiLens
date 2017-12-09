using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

namespace Assets
{
    public class CognitiveService : MonoBehaviour
    {
        private const string ConversaationEndpoint = "https://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US";
        private const string TokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

        private string _subscriptionKey;
        private const string RequestSubscrpitionHeaderKey = "Ocp-Apim-Subscription-Key";
        private string _token;
        private WebSocket _webSocket;
        public void StartConnection()
        { 
            _webSocket =  new WebSocket(ConversaationEndpoint);
            _webSocket.OnMessage += (s, e) =>
            {
                
            };
            _webSocket.OnOpen += (s, e) =>
            {
                
            };
            _webSocket.Connect();
        }

        public void Connect(string subscriptionKey)
        {
            var coroutine = RequestToken(subscriptionKey);
            StartCoroutine(coroutine);
        }

        public IEnumerator RequestToken(string subscriptionKey)
        {
            var request = UnityWebRequest.Post(TokenEndpoint, subscriptionKey);
            request.SetRequestHeader(RequestSubscrpitionHeaderKey, _subscriptionKey);
            yield return request.Send();
            _token = request.downloadHandler.text;
        }
    }
}
