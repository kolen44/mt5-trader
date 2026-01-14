using System;
using System.Runtime.Serialization;

namespace mtapi.mt5
{
    [Serializable]
    public class InvestorModeException : Exception
    {
        public InvestorModeException()
        {
        }

        public InvestorModeException(string message) : base(message)
        {
        }

        public InvestorModeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvestorModeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}