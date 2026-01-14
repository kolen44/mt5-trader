using System;
using System.Runtime.Serialization;

namespace mtapi.mt5
{
    [Serializable]
    internal class TradeTimeoutException : TimeoutException
    {
        public TradeTimeoutException()
        {
        }

        public TradeTimeoutException(string message) : base(message)
        {
        }

        public TradeTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TradeTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}