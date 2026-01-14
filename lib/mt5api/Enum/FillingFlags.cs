using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	[Flags]
	public enum FillingFlags
	{

		NONE = 0,
		FOK = 1,
		IOC = 2,
		BOC = 4
	}
}
