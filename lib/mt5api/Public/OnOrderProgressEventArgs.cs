using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Order progress event arguments.
    /// </summary>
    public struct OrderProgressEventArgs
    {
        /// <summary>
        /// Temporary ID. Useful until server assign ticket number.
        /// </summary>
        public int TempID;
        /// <summary>
        /// Stage of order processing by server.
        /// </summary>
        public ProgressType Type;
        /// <summary>
        /// Opened/closed order.
        /// </summary>
        //public Order Order;
        /// <summary>
        /// Exception during processing of the order. Could be ServerException or RequoteException.
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>"TempID Type Exception"</returns>
        public override string ToString()
        {
            string res = TempID + " " + Type + " " + Exception;
            if (Exception != null)
                res += " " + Exception.Message;
            return res;
        }
    }
}