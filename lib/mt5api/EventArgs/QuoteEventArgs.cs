using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// New quote event arguments.
    /// </summary>
    public class Quote
    {
        /// <summary> 
        /// Trading instrument.
        /// </summary>
        public string Symbol;
        /// <summary>
        /// Bid.
        /// </summary>
        public double Bid;
        /// <summary>
        /// Ask.
        /// </summary>
        public double Ask;
        /// <summary>
        /// Server time.
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// Last deal price.
        /// </summary>
        public double Last;
        /// <summary>
        /// Volume
        /// </summary>
        public ulong Volume;
        internal ulong UpdateMask;
        internal short BankId;
		internal readonly DateTime CreationTime = DateTime.Now;

		/// <summary>
		/// Convert to string.
		/// </summary>
		/// <returns>"Symbol Bid Ask"</returns>
		public override string ToString()
        {
            return Symbol + " " + Bid + " " + Ask;
        }
    }
}