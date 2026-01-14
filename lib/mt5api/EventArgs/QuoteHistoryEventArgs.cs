using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{

    /// <summary>
    /// Quote history event args.
    /// </summary>
    public struct QuoteHistoryEventArgs
    {
        /// <summary>
        /// Instrument.
        /// </summary>
        public string Symbol;
        /// <summary>
        /// History bars.
        /// </summary>
        public List<Bar> Bars;
    }


	/// <summary>
	/// Tick history event args.
	/// </summary>
	public struct TickHistoryEventArgs
	{
		/// <summary>
		/// Instrument.
		/// </summary>
		public string Symbol;
		/// <summary>
		/// History bars.
		/// </summary>
		public TickBar[] Bars;
	}
}
