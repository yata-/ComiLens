using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Assets.SpeechClient
{
    public class Payload
    {
        public string RequestId { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public string Content{ get; set; }

    }

    public class PayloadParser
    {
        public Payload Parse(string value)
        {
            System.IO.StringReader rs = new System.IO.StringReader(value);
            var requestId = rs.ReadLine().Split(':')[1];
            var contentType = rs.ReadLine().Split(':')[1];
            var path = rs.ReadLine().Split(':')[0];
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