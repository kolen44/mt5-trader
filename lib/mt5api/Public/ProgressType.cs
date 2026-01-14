using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Stage of order processing by server.
    /// </summary>
    public enum ProgressType
    {
        /// <summary>
        /// Order was rejected.
        /// </summary>
        Rejected,
        /// <summary>
        /// Order was accepted by server.
        /// </summary>
        Accepted,
        /// <summary>
        /// Server started to execute the order.
        /// </summary>
        InProcess,
        /// <summary>
        /// Order was opened.
        /// </summary>
        Opened,
        /// <summary>
        /// Order was closed.
        /// </summary>
        Closed,
        /// <summary>
        /// Order was modified.
        /// </summary>
        Modified,
        /// <summary>
        /// Pending order was deleted.
        /// </summary>
        PendingDeleted,
        /// <summary>
        /// Closed of pair of opposite orders.
        /// </summary>
        ClosedBy,
        /// <summary>
        /// Closed of multiple orders.
        /// </summary>
        MultipleClosedBy,
        /// <summary>
        /// Trade timeout.
        /// </summary>
        Timeout,
        /// <summary>
        /// Price data.
        /// </summary>
        Price,
        /// <summary>
        /// Exception.
        /// </summary>
        Exception
    }
}
