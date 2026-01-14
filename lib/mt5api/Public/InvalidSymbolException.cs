using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Invalid symbol exception
    /// </summary>
    public class InvalidSymbolException : Exception
    {
        /// <summary>
        /// Initialize TradeTimeoutException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InvalidSymbolException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Invalid symbol exception
    /// </summary>
    public class DoubleRequestException : Exception
    {
        /// <summary>
        /// Initialize TradeTimeoutException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public DoubleRequestException(string message)
            : base(message)
        {
        }
    }
}
