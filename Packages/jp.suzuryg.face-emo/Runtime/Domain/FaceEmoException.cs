using System;
using System.Runtime.Serialization;

namespace Suzuryg.FaceEmo.Domain
{
    [Serializable()]
    public class FaceEmoException : Exception
    {
        public FaceEmoException()
            : base()
        {
        }

        public FaceEmoException(string message)
            : base(message)
        {
        }

        public FaceEmoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FaceEmoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
