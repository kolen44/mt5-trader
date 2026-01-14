using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace mtapi.mt5
{
	/// <summary>
	/// Error reply from server on login and order opening/closing/modifying.
	/// </summary>
	public class ServerException : Exception
	{
		/// <summary>
		/// Error code.
		/// </summary>
		public readonly Msg Code;

		/// <summary>
		/// Initialize ServerException.
		/// </summary>
		/// <param name="code">Exception code.</param>
		public ServerException(Msg code)
			: base(TradeCodes.GetMesssage(code))
		{
			Code = code;
		}

        /// <summary>
        /// Initialize ServerException.
        /// </summary>
        /// <param name="code">Exception code.</param>
        public ServerException(Msg code, string msg)
            : base(msg + " " + code.ToString())
        {
            Code = code;
        }
    }

	internal class TradeCodes
	{
		static ConcurrentDictionary<int, string> Codes = new ConcurrentDictionary<int, string>();

        internal static string GetMesssage(Msg code)
        {
            if(Codes.TryGetValue((int)code, out var msg))
                return msg;
            else
                return code.ToString();
        }
		static TradeCodes()
		{
            Codes[10004] = "Requote";
            Codes[10006] = "Request rejected";
            Codes[10007] = "Request canceled by trader";
            Codes[10008] = "Order placed";
            Codes[10009] = "Request completed";
            Codes[10010] = "Only part of the request was completed";
            Codes[10011] = "Request processing error";
            Codes[10012] = "Request canceled by timeout";
            Codes[10013] = "Invalid request";
            Codes[10014] = "Invalid volume in the request";
            Codes[10015] = "Invalid price in the request";
            Codes[10016] = "Invalid stops in the request";
            Codes[10017] = "Trade is disabled";
            Codes[10018] = "Market is closed";
            Codes[10019] = "There is not enough money to complete the request";
            Codes[10020] = "Prices changed";
            Codes[10021] = "There are no quotes to process the request";
            Codes[10022] = "Invalid order expiration date in the request";
            Codes[10023] = "Order state changed";
            Codes[10024] = "Too frequent requests";
            Codes[10025] = "No changes in request";
            Codes[10026] = "Autotrading disabled by server";
            Codes[10027] = "Autotrading disabled by client terminal";
            Codes[10028] = "Request locked for processing";
            Codes[10029] = "Order or position frozen";
            Codes[10030] = "Invalid order filling type";
            Codes[10031] = "No connection with the trade server";
            Codes[10032] = "Operation is allowed only for live accounts";
            Codes[10033] = "The number of pending orders has reached the limit";
            Codes[10034] = "The volume of orders and positions for the symbol has reached the limit";
            Codes[10035] = "Incorrect or prohibited order type";
            Codes[10036] = "Position with the specified POSITION_IDENTIFIER has already been closed";
            Codes[10038] = "A close volume exceeds the current position volume";
            Codes[10039] = "A close order already exists for a specified position. This may happen when working in the hedging system:" +
            Environment.NewLine + "when attempting to close a position with an opposite one, while close orders for the position already exist" +
            Environment.NewLine + "when attempting to fully or partially close a position if the total volume of the already present close orders and the newly placed one exceeds the current position volume";
            Codes[10040] = "The number of open positions simultaneously present on an account can be limited by the server settings. After a limit is reached, the server returns the TRADE_RETCODE_LIMIT_POSITIONS error when attempting to place an order. The limitation operates differently depending on the position accounting type:" +
            Environment.NewLine + "Netting — number of open positions is considered. When a limit is reached, the platform does not let placing new orders whose execution may increase the number of open positions. In fact, the platform allows placing orders only for the symbols that already have open positions. The current pending orders are not considered since their execution may lead to changes in the current positions but it cannot increase their number." +
            Environment.NewLine + "Hedging — pending orders are considered together with open positions, since a pending order activation always leads to opening a new position. When a limit is reached, the platform does not allow placing both new market orders for opening positions and pending orders.";
            Codes[10041] = "The pending order activation request is rejected, the order is canceled";
            Codes[10042] = "The request is rejected, because the \"Only long positions are allowed\" rule is set for the symbol (POSITION_TYPE_BUY)";
            Codes[10043] = "The request is rejected, because the \"Only short positions are allowed\" rule is set for the symbol (POSITION_TYPE_SELL)";
            Codes[10044] = "The request is rejected, because the \"Only position closing is allowed\" rule is set for the symbol";
            Codes[10045] = "The request is rejected, because \"Position closing is allowed only by FIFO rule\" flag is set for the trading account (ACCOUNT_FIFO_CLOSE=true)";
            Codes[10046] = "The request is rejected, because the \"Opposite positions on a single symbol are disabled\" rule is set for the trading account. For example, if the account has a Buy position, then a user cannot open a Sell position or place a pending sell order. The rule is only applied to accounts with hedging accounting system (ACCOUNT_MARGIN_MODE=ACCOUNT_MARGIN_MODE_RETAIL_HEDGING).";

        }
    }
}
