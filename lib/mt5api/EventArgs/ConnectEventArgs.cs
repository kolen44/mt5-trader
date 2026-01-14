using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Connect event argumnets.
    /// </summary>
    public class ConnectEventArgs
    {
        /// <summary>
        /// Connect exception. Can be null.
        /// </summary>
        public Exception Exception;
        /// <summary>
        /// Connect progress
        /// </summary>
        public ConnectProgress Progress;
    }
    /// <summary>
    /// Connect progress
    /// </summary>
    public enum ConnectProgress
    {
        SendLogin, SendAccountPassword, AcceptAuthorized, RequestTradeInfo, Connected, Exception, Disconnect
    }
}
