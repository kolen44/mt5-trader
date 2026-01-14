using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Fill policy
    /// </summary>
    public enum FillPolicy
    {
        FillOrKill = 0,
        ImmediateOrCancel = 1,
        FlashFill = 2,
        Any = 3
    }
}


//#define FILLING_FILL_OR_KILL		1
//#define FILLING_IMMEDIATE_OR_CANCEL	2
//#define FILLING_FLASH_FILL			4
//#define FILLING_ALL					3