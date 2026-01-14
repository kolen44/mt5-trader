using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Profit calculation mode
    /// </summary>
    public enum CalculationMode
    {
        Forex = 0,
        Futures = 1,
        CFD = 2,
        CFDIndex = 3,
        CFDLeverage = 4,
        CalcMode5 = 5,
        ExchangeStocks = 32,
        ExchangeFutures = 33,
        FORTSFutures = 34,
        ExchangeOption = 35,
        ExchangeMarginOption = 36,
        ExchangeBounds = 37,
        Collateral = 64
    }
    /// <summary>
    /// Margin calculation mode
    /// </summary>
    public enum MarginMode
    {
        MarginForex = 0,
        MarginFutures = 1,
        vMarginCFD = 2,
        MarginCFDIndex = 3
    }
    /// <summary>
    /// Trade mode
    /// </summary>
    public enum TradeMode
    {
        Disabled = 0,
        LongOnly = 1,
        ShortOnly = 2,
        CloseOnly = 3,
        FullAccess = 4
    }
    /// <summary>
    /// GTC mode
    /// </summary>
    public enum GTCMode //Good till
    {
        Cancelled = 0,
        TodayIncludeSL_TP = 1,
        TodayExcludeSL_TP = 2
    }
    /// <summary>
    /// Swap type
    /// </summary>
    public enum SwapType
    {
        SwapNone = 0,
        InPoints = 1,
        SymInfo_s408 = 2, //???
        MarginCurrency = 3,
        Currency = 4,
        PercCurPrice = 5, //In percentage terms, using current price
        PercOpenPrice = 6, //In percentage terms, using open price
        PointClosePrice = 7, //In points, reopen position by close price
        PointBidPrice = 8 //In points, reopen position by bid price
    }
    /// <summary>
    /// Swap day
    /// </summary>
    public enum V3DaysSwap
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }
    /// <summary>
    /// Execution type
    /// </summary>
    public enum ExecutionType
    {
        Request = 0,
        Instant = 1,
        Market = 2,
        Exchange = 3
    }

    /// <summary>
    /// Netting or Hedging
    /// </summary>
    public enum AccMethod
    {
        Default = 0,
        Netting = 1,
        Hedging = 2
    }

}
