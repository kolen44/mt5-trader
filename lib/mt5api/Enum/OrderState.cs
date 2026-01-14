using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Order state
    /// </summary>
    public enum OrderState
    {
        Started = 0,
        Placed = 1,
        Cancelled = 2,
        Partial = 3,
        Filled = 4,
        Rejected = 5,
        Expired = 6,
        RequestAdding = 7,
        RequestModifying = 8,
        RequestCancelling = 9
    }
}
