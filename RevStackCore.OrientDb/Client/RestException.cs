using System;

namespace RevStackCore.OrientDb.Client
{
    public class RestException : Exception
    {
        public RestException() : base() { }
        public RestException(string message) : base(message) { }
        public RestException(string message, Exception innerException) : base(message, innerException) { }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
    }
}
