using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace mtapi.mt5
{

    [DefaultValue(Default)]
    public enum PlacedType
    {
        /// <summary>
        /// The deal was executed as a result of activation of an order placed from a desktop terminal
        /// </summary>
        Manually = 0,
        /// <summary>
        /// The deal was executed as a result of activation of an order placed from a mobile application
        /// </summary>
        Mobile = 16,
        /// <summary>
        /// The deal was executed as a result of activation of an order placed from the web platform
        /// </summary>
        Web = 17,
        /// <summary>
        /// The deal was executed as a result of activation of an order placed from an MQL5 program, i.e. an Expert Advisor or a script
        /// </summary>
        ByExpert = 1,
        /// <summary>
        /// The deal was executed as a result of Stop Loss activation
        /// </summary>
        OnSL = 3,
        /// <summary>
        /// The deal was executed as a result of Take Profit activation
        /// </summary>
        OnTP = 4,
        /// <summary>
        /// The deal was executed as a result of the Stop Out event
        /// </summary>
        OnStopOut = 5,
        /// <summary>
        /// The deal was executed due to a rollover
        /// </summary>
        OnRollover = 6,
        /// <summary>
        /// The deal was executed after charging the variation margin
        /// </summary>
        OnVmargin = 8,
        /// <summary>
        /// The deal was executed after the split (price reduction) of an instrument, which had an open position during split announcement
        /// </summary>
        OnSplit = 18,
        /// <summary>
        /// In this case API uses MT5API.PlacedType field during OrderSend and OrderClose
        /// </summary>
        ByDealer = 2,
		/// <summary>
		/// The order has been placed by the MetaTrader 5 gateway connected to the platform.
		/// </summary>
		Gateway = 9,
		/// <summary>
		/// The order has been placed as a result of copying a trading signal according to the subscription in the client terminal.
		/// </summary>
		Signal = 10,
		/// <summary>
		/// The order was placed as a result of operations connected with the futures/options delivery date coming into effect. It is currently not used. It is currently not used.
		/// </summary>
		Settlement = 11,
		/// <summary>
		/// Order placed as a result of relocating a position with a calculated price to a new symbol with the same underlying asset. It is currently not used.
		/// </summary>
		Transfer = 12,
		/// <summary>
		/// The order was placed while synchronizing a trading account with an external system.
		/// </summary>
		Sync = 13,
		/// <summary>
		/// The order was placed from the external trading system for operational purposes (for example, to correct a trading status).
		/// </summary>
		ExternalService = 14,
		/// <summary>
		/// The order was created while importing clients' trading operations from the MetaTrader 4 server.
		/// </summary>
		Migration = 15,
        Default = 20
    }

}
