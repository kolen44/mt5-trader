using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Expiration type
    /// </summary>
    public enum ExpirationType
    {
        GTC = 0,
        Today = 1,
        Specified = 2,
        SpecifiedDay = 3
    }
}
