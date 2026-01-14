using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	/// <summary>
	/// Details of trade progress
	/// </summary>
    public class OrderProgress : FromBufReader
    {
        public TransactionInfo OrderUpdate;
        public TradeRequest TradeRequest;
        public TradeResult TradeResult;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 0;
			var st = new OrderProgress();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
