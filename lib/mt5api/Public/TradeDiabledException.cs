//#define NOREQUOTE

using System;
using System.Runtime.Serialization;

namespace mtapi.mt5
{
    [Serializable]
    internal class TradeDiabledException : Exception
    {
        public TradeDiabledException()
        {
        }

        public TradeDiabledException(string message) : base(message)
        {
        }

        public TradeDiabledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TradeDiabledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}