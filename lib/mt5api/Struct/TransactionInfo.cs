using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x98, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Transaction information
    /// </summary>
    public class TransactionInfo : FromBufReader
    {
        /// <summary>
        /// Transaction ticket
        /// </summary>
        /*[FieldOffset(0)]*/ public int UpdateId;
        /// <summary>
        /// Order ticket
        /// </summary>
        /*[FieldOffset(4)]*/ public int Action;
        /// <summary>
        /// Deal ticket
        /// </summary>
        /*[FieldOffset(8)]*/ public long TicketNumber;
        /*[FieldOffset(16)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Currency;
        /// <summary>
        /// Symbol currency
        /// </summary>
        /*[FieldOffset(80)]*/ public int Id;
        /// <summary>
        /// Significant digits
        /// </summary>
        /*[FieldOffset(84)]*/ private int s54;
        /// <summary>
        /// Transaction id
        /// </summary>
        /*[FieldOffset(88)]*/ public OrderType s58;
        /// <summary>
        /// Transaction type
        /// </summary>
        /*[FieldOffset(92)]*/ private int s5C;
        /// <summary>
        /// Order type
        /// </summary>
        /*[FieldOffset(96)]*/ public OrderState OrderState;
        /// <summary>
        /// Order state
        /// </summary>
        /*[FieldOffset(100)]*/ public ExpirationType ExpirationType;
        /// <summary>
        /// Order placed type
        /// </summary>
        /*[FieldOffset(104)]*/ public long ExpirationTime;
        /// <summary>
        /// Deal type
        /// </summary>
        /*[FieldOffset(112)]*/ public double OpenPrice;
        /// <summary>
        /// Deal placed type
        /// </summary>
        /*[FieldOffset(120)]*/ public double OrderPrice;
        /// <summary>
        /// Expiration type
        /// </summary>
        /*[FieldOffset(128)]*/ public double StopLoss;
        /// <summary>
        /// Expiration time
        /// </summary>
        /*[FieldOffset(136)]*/ public double TakeProfit;
        /// <summary>
        /// Open price
        /// </summary>
        /*[FieldOffset(144)]*/ public ulong Volume;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 152;
			var st = new TransactionInfo();
			st.UpdateId = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Action = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TicketNumber = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Currency = GetString(buf.Bytes(64));
			st.Id = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s54 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s58 = (OrderType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s5C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.OrderState = (OrderState)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.ExpirationType = (ExpirationType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.ExpirationTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.OpenPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.OrderPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.StopLoss = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.TakeProfit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
