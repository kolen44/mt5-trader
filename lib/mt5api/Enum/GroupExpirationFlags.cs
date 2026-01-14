using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace mtapi.mt5
{
	[Flags]
	public enum ExpirationFlags
	{
		/// <summary>
		/// All expiration types are disabled.
		/// </summary>
		NONE = 0,
		/// <summary>
		/// Orders are good till canceled.
		/// </summary>
		GTC = 1,
		/// <summary>
		/// Orders are effective only during the current trading day.
		/// </summary>
		DAY = 2,
		/// <summary>
		/// Orders are effective till the date specified by the trader.
		/// </summary>
		SPECIFIED = 4,
		/// <summary>
		/// Orders with expiration at a specified day.An order expires at 00:00 of a specified day or at a nearest trade time. The expiration time is 00:00 of the specified day or the nearest trading time.
		/// </summary>
		SPECIFIED_DAY = 8,
		/// <summary>
		/// All flags are enabled.
		/// </summary>
		ALL = 15
	}
}
