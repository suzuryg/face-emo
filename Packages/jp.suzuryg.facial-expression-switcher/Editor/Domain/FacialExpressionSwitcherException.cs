using System;
using System.Runtime.Serialization;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    [Serializable()]
    public class FacialExpressionSwitcherException : Exception
    {
        public FacialExpressionSwitcherException()
            : base()
        {
        }

        public FacialExpressionSwitcherException(string message)
            : base(message)
        {
        }

        public FacialExpressionSwitcherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FacialExpressionSwitcherException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
