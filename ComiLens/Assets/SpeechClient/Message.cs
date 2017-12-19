using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace Assets.SpeechClient
{
    // Speech Service WebSocket protocolのプロトコル
    // https://docs.microsoft.com/ja-jp/azure/cognitive-services/speech/api-reference-rest/websocketprotocol
    public class Payload
    {
        private const string SpeechPrefix = "speech.";

        public string RequestId { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public string Content{ get; set; }

        public PayloadType Type
        {
            get
            {
                if (this.Path.StartsWith(SpeechPrefix) == false)
                {
                    return PayloadType.None;
                }
                var path = Path.Remove(0, SpeechPrefix.Length);
                return (PayloadType) Enum.Parse(typeof(PayloadType), path, true);
            }
        }

        public Message GetMessage()
        {
            return JsonConvert.DeserializeObject<Message>(Content);
        }
    }

    public enum PayloadType
    {
        None,
        StartDetected,
        Hypothesis,
        Phrase,
        EndDetected

    }

    public enum RecognitionStatus
    {
        Success,
        NoMatch,
        InitialSilenceTimeout,
        BabbleTimeout,
        Error
    }

    public class Message
    {
        public RecognitionStatus Status
        {
            get
            {
                return (RecognitionStatus)Enum.Parse(typeof(RecognitionStatus), RecognitionStatus, true);
            }
        }
        public string RecognitionStatus { get; set; }
        public int Offset { get; set; }

        public int Duration { get; set; }
        public string DisplayText { get; set; }
        public string Text { get; set; }

    }

    public class PayloadParser
    {
        public Payload Parse(string value)
        {
            var rs = new StringReader(value);
            var requestId = rs.ReadLine().Split(':')[1];
            var contentType = rs.ReadLine().Split(':')[1];
            var path = rs.ReadLine().Split(':')[1];
            var result = rs.ReadToEnd();
            var message = new Payload()
            {
                RequestId =  requestId,
                ContentType = contentType,
                Path = path,
                Content = result
            };
            return message;
        }
    }
}