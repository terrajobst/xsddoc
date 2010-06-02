using System;
using System.Runtime.Serialization;

namespace XsdDocumentation
{
    [Serializable]
    public class XsdDocumentationException : Exception
    {
        public XsdDocumentationException()
        {
        }

        public XsdDocumentationException(string message)
            : base(message)
        {
        }

        public XsdDocumentationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected XsdDocumentationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}