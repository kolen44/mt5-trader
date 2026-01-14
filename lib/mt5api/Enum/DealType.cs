using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Deal type
    /// </summary>
    public enum DealType
    {
        DealBuy = 0,
        DealSell = 1,
        Balance = 2,
        Credit = 3,
        Charge = 4,
        Correction = 5,
        Bonus = 6,
        Commission = 7,
        DailyCommission = 8,
        MonthlyCommission = 9,
        DailyAgentCommission = 10,
        MonthlyAgentCommission = 11,
        InterestRate = 12,
        CanceledBuy = 13,
        CanceledSell = 14,
        Dividend = 15,
        FrankedDividend = 16,
        Tax = 17,
        AgentCommission = 18,
        SoCompensation = 19,
        SoCreditCompensation = 20
    }
}
