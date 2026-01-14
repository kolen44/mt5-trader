using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Order type
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Buy order
        /// </summary>
        Buy = 0,
        /// <summary>
        /// Sell order  
        /// </summary>
        Sell = 1,
        /// <summary>
        /// Buy limit order
        /// </summary>
        BuyLimit = 2,
        /// <summary>
        /// Sell limit order
        /// </summary>
        SellLimit = 3,
        /// <summary>
        /// Buy stop order
        /// </summary>
        BuyStop = 4,
        /// <summary>
        /// Sell stop order
        /// </summary>
        SellStop = 5,
        /// <summary>
        /// Buy stop limit order
        /// </summary>
        BuyStopLimit = 6,
        /// <summary>
        /// Sell stop limit order
        /// </summary>
        SellStopLimit = 7,
        /// <summary>
        /// Close by order
        /// </summary>
        CloseBy = 8,
        /// <summary>
        /// Balance order
        /// </summary>
        Balance = 100,
        /// <summary>
        /// Credit order
        /// </summary>
        Credit = 101
    }
}
