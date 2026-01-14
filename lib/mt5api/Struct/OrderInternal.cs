using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x27C, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Internal information for the order(pending)
    /// </summary>
    public class OrderInternal : FromBufReader
    {
        /// <summary>
        /// Ticket number
        /// </summary>
        /*[FieldOffset(0)]*/
        public long TicketNumber;
		/// <summary>
		/// Ticket number
		/// </summary>
		/*[FieldOffset(0)]*/
		public long Ticket => TicketNumber;
		/// <summary>
		/// Text id
		/// </summary>
		/*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string Id;
        /// <summary>
        /// Account login
        /// </summary>
        /*[FieldOffset(72)]*/
        public ulong Login;
        /*[FieldOffset(80)]*/
        private long s50;
        /// <summary>
        /// Symbol currency
        /// </summary>
        /*[FieldOffset(88)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        public string Symbol;
        /// <summary>
        /// History time	(as FileTime format)
        /// </summary>
        /*[FieldOffset(152)]*/
        public long HistoryTime;
        /// <summary>
        /// Open time
        /// </summary>
        /*[FieldOffset(160)]*/
        public long OpenTime;
        /// <summary>
        /// Expiration time
        /// </summary>
        /*[FieldOffset(168)]*/
        public long ExpirationTime;
        /// <summary>
        /// Execution time
        /// </summary>
        /*[FieldOffset(176)]*/
        public long ExecutionTime;
        public DateTime ExecutionTimeAsDateTime => ConvertTo.DateTime(ExecutionTime);
        /// <summary>
        /// Order type
        /// </summary>
        /*[FieldOffset(184)]*/
        public OrderType Type;
        /// <summary>
        /// Fill policy
        /// </summary>
        /*[FieldOffset(188)]*/
        public FillPolicy FillPolicy;
        /// <summary>
        /// Expiration type
        /// </summary>
        /*[FieldOffset(192)]*/
        public ExpirationType ExpirationType;
        /*[FieldOffset(196)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/
        private byte[] sC4;
        /// <summary>
        /// Placed type
        /// </summary>
        /*[FieldOffset(204)]*/
        public PlacedType PlacedType;
        /*[FieldOffset(208)]*/
        private int sD0;
        /// <summary>
        /// Open price
        /// </summary>
        /*[FieldOffset(212)]*/
        public double OpenPrice;
        /// <summary>
        /// StopLimit price
        /// </summary>
        /*[FieldOffset(220)]*/
        public double StopLimitPrice;
        /// <summary>
        /// Price
        /// </summary>
        /*[FieldOffset(228)]*/
        public double Price;
        /// <summary>
        /// Stop loss
        /// </summary>
        /*[FieldOffset(236)]*/
        public double StopLoss;
        /// <summary>
        /// Take profit
        /// </summary>
        /*[FieldOffset(244)]*/
        public double TakeProfit;
        /// <summary>
        /// Lots
        /// </summary>
        public double Lots
        {
            get { return (double)Volume / 100000000; }
        }

        /// <summary>
        /// Cover volume
        /// </summary>
        /*[FieldOffset(252)]*/
        public ulong Volume;

        

        /// <summary>
        /// Request lots
        /// </summary>
        public double RequestLots
        {
            get { return (double)RequestVolume / 100000000; }
        }
        /// <summary>
        /// Request volume
        /// </summary>
        /*[FieldOffset(260)]*/
        public ulong RequestVolume;
        /// <summary>
        /// Order state
        /// </summary>
        /*[FieldOffset(268)]*/
        public OrderState State;
        /// <summary>
        /// Expert id
        /// </summary>
        /*[FieldOffset(272)]*/
        public long ExpertId;
        /// <summary>
        /// Associative deal ticket
        /// </summary>
        /*[FieldOffset(280)]*/
        public long DealTicket;
        /// <summary>
        /// Comment text
        /// </summary>
        /*[FieldOffset(288)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        public string Comment;
        /// <summary>
        /// Lots
        /// </summary>
        /*[FieldOffset(352)]*/
        public double ContractSize;
        /// <summary>
        /// Significant digits
        /// </summary>
        /*[FieldOffset(360)]*/
        public int Digits;
        /// <summary>
        /// Symbols base significant digits
        /// </summary>
        /*[FieldOffset(364)]*/
        public int BaseDigits;
        /*[FieldOffset(368)]*/
        private double s170;
        /*[FieldOffset(376)]*/
        private double s178;
        /*[FieldOffset(384)]*/
        private long s180;
        /// <summary>
        /// Profit rate
        /// </summary>
        /*[FieldOffset(392)]*/
        public double ProfitRate;
        /// <summary>
        /// Open time (ms)
        /// </summary>
        /*[FieldOffset(400)]*/
        public long OpenTimeMs;
        public DateTime OpenTimeMsAsDateTime => ConvertTo.DateTimeMs(OpenTimeMs);
        public DateTime OpenTimeAsDateTime => ConvertTo.DateTimeMs(OpenTimeMs);
        /*[FieldOffset(408)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 48)]*/
        private byte[] s198;
        /*[FieldOffset(456)]*/
        internal int s1C8;
        /*[FieldOffset(460)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 176)]*/
        private byte[] s1CC;
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 636;
            var st = new OrderInternal();
            st.TicketNumber = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Id = GetString(buf.Bytes(64));
            st.Login = BitConverter.ToUInt64(buf.Bytes(8), 0);
            st.s50 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Symbol = GetString(buf.Bytes(64));
            st.HistoryTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.OpenTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.ExpirationTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.ExecutionTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Type = (OrderType)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.FillPolicy = (FillPolicy)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.ExpirationType = (ExpirationType)BitConverter.ToInt32(buf.Bytes(4), 0);
            //st.sC4 = new byte[8];
            //for (int i = 0; i < 8; i++)
            //    st.sC4[i] = buf.Byte();
            buf.Bytes(8);
            st.PlacedType = (PlacedType)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.sD0 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.OpenPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.StopLimitPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Price = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.StopLoss = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.TakeProfit = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
            st.RequestVolume = BitConverter.ToUInt64(buf.Bytes(8), 0);
            st.State = (OrderState)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.ExpertId = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.DealTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Comment = GetString(buf.Bytes(64));
            st.ContractSize = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Digits = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.BaseDigits = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s170 = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.s178 = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.s180 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.ProfitRate = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.OpenTimeMs = BitConverter.ToInt64(buf.Bytes(8), 0);
            //st.s198 = new byte[48];
            //for (int i = 0; i < 48; i++)
            //    st.s198[i] = buf.Byte();
            buf.Bytes(48);
            st.s1C8 = BitConverter.ToInt32(buf.Bytes(4), 0);
            //st.s1CC = new byte[176];
            //for (int i = 0; i < 176; i++)
            //    st.s1CC[i] = buf.Byte();
            buf.Bytes(176);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
        public bool IsAssociativeDealOrder()
        {
            return IsTradeOrder() && DealTicket!=0 && (TicketNumber == 0 || (DealTicket != TicketNumber));
        }

        public bool IsTradeOrder()
        {
            return (Type == OrderType.Buy) || (Type == OrderType.Sell);
        }

        public bool IsLimitOrder()
        {
            return (Type == OrderType.BuyLimit) || (Type == OrderType.SellLimit);
        }

        public bool IsStopOrder()
        {
            return (Type == OrderType.BuyStop) || (Type == OrderType.SellStop);
        }

        public bool IsBuyOrder()
        {
            return (Type == OrderType.Buy) || (Type == OrderType.BuyStop) || (Type == OrderType.BuyLimit) || (Type == OrderType.BuyStopLimit);
        }

        public bool IsStopLimitOrder()
        {
            return (Type == OrderType.BuyStopLimit) || (Type == OrderType.SellStopLimit);
        }

        public bool AreEquals(OrderInternal other)
        {
            if (other == null) return false;

            return
                TicketNumber == other.TicketNumber &&
                Id == other.Id &&
                Login == other.Login &&
                Symbol == other.Symbol &&
                HistoryTime == other.HistoryTime &&
                OpenTime == other.OpenTime &&
                ExpirationTime == other.ExpirationTime &&
                ExecutionTime == other.ExecutionTime &&
                Type == other.Type &&
                FillPolicy == other.FillPolicy &&
                ExpirationType == other.ExpirationType &&
                PlacedType == other.PlacedType &&
                OpenPrice == other.OpenPrice &&
                StopLimitPrice == other.StopLimitPrice &&
                Price == other.Price &&
                StopLoss == other.StopLoss &&
                TakeProfit == other.TakeProfit &&
                Volume == other.Volume &&
                RequestVolume == other.RequestVolume &&
                State == other.State &&
                ExpertId == other.ExpertId &&
                DealTicket == other.DealTicket &&
                Comment == other.Comment &&
                ContractSize == other.ContractSize &&
                Digits == other.Digits &&
                BaseDigits == other.BaseDigits &&
                ProfitRate == other.ProfitRate &&
                OpenTimeMs == other.OpenTimeMs;
        }
    }

}
