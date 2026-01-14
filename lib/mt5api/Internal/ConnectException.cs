using System;
using System.Runtime.Serialization;

namespace mtapi.mt5
{
	[Serializable]
	public class ConnectException : Exception
	{
		public ConnectException() : base("Not connected")
		{
		}

		public ConnectException(string message) : base(message)
		{
		}
	}

}