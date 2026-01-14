using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x2A0, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Internal details for the deal
    /// </summary>
    public class DealInternal : FromBufReader
    {
        /// <summary>
        /// Deal ticket
        /// </summary>
        /*[FieldOffset(0)]*/
        public long TicketNumber;
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
        /// <summary>
        /// History time (as FileTime format)
        /// </summary>
        /*[FieldOffset(80)]*/
        public long HistoryTime;
        /// <summary>
        /// Order ticket
        /// </summary>
        /*[FieldOffset(88)]*/
        public long OrderTicket;
        /*[FieldOffset(96)]*/
        private long s60;
        /// <summary>
        /// Open time
        /// </summary>
        public DateTime OpenTimeAsDateTime
        {
            get
            {
                return ConvertTo.DateTime(OpenTime);
            }
        }
        /// <summary>
        /// Open time
        /// </summary>
        /*[FieldOffset(104)]*/
        public long OpenTime;
        /*[FieldOffset(112)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/
        private byte[] s70;
        /// <summary>
        /// Symbol currency
        /// </summary>
        /*[FieldOffset(120)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        public string Symbol;
        /// <summary>
        /// Deal type
        /// </summary>
        /*[FieldOffset(184)]*/
        public DealType Type;
        /// <summary>
        /// Deal direction
        /// </summary>
        /*[FieldOffset(188)]*/
        public Direction Direction;
        /// <summary>
        /// Open price
        /// </summary>
        /*[FieldOffset(192)]*/
        public double OpenPrice;
        /// <summary>
        /// Price
        /// </summary>
        /*[FieldOffset(200)]*/
        public double Price;
        /// <summary>
        /// Stop loss
        /// </summary>
        /*[FieldOffset(208)]*/
        public double StopLoss;
        /// <summary>
        /// Take profit
        /// </summary>
        /*[FieldOffset(216)]*/
        public double TakeProfit;

        /// <summary>
        /// Lots
        /// </summary>
        public double Lots
        {
            get
            {
                return (double)Volume / 100000000;
            }
        }


        /// <summary>
        /// Volume
        /// </summary>
        /*[FieldOffset(224)]*/
        public ulong Volume;
        /// <summary>
        /// Profit (money digits)
        /// </summary>
        /*[FieldOffset(232)]*/
        public double Profit;
        /// <summary>
        /// Profit rate
        /// </summary>
        /*[FieldOffset(240)]*/
        public double ProfitRate;
        /// <summary>
        /// Volume rate
        /// </summary>
        /*[FieldOffset(248)]*/
        public double VolumeRate;
        /// <summary>
        /// Commission (money digits)
        /// </summary>
        /*[FieldOffset(256)]*/
        public double Commission;
        /*[FieldOffset(264)]*/
        public double Fee;
        /// <summary>
        /// Swap
        /// </summary>
        /*[FieldOffset(272)]*/
        public double Swap;
        /// <summary>
        /// Expert id
        /// </summary>
        /*[FieldOffset(280)]*/
        public long ExpertId;
        /// <summary>
        /// Position ticket
        /// </summary>
        /*[FieldOffset(288)]*/
        public long PositionTicket;
        /// <summary>
        /// Text comment
        /// </summary>
        /*[FieldOffset(296)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        public string Comment;
        /// <summary>
        /// Lots
        /// </summary>
        /*[FieldOffset(360)]*/
        public double ContractSize;
        /// <summary>
        /// Significant digits
        /// </summary>
        /*[FieldOffset(368)]*/
        public int Digits;
        /// <summary>
        /// Money significant digits
        /// </summary>
        /*[FieldOffset(372)]*/
        public int MoneyDigits;
        /*[FieldOffset(376)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 132)]*/
        private byte[] s178;
        /// <summary>
        /// Free profit
        /// </summary>
        /*[FieldOffset(508)]*/
        public double FreeProfit;
        /*[FieldOffset(516)]*/
        private long s204;
        /*[FieldOffset(524)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/
        private byte[] s20C;
        /// <summary>
        /// Trail rounder
        /// </summary>
        /*[FieldOffset(532)]*/
        public double TrailRounder;
        /*[FieldOffset(540)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/
        private byte[] s21C;
        /// <summary>
        /// Open time (ms)
        /// </summary>
        /*[FieldOffset(548)]*/
        public long OpenTimeMs;
        /*[FieldOffset(556)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/
        private byte[] s22C;
        /*[FieldOffset(564)]*/
        private long s234;
        /*[FieldOffset(572)]*/
        private long s23C;
        /*[FieldOffset(580)]*/
        private long s244;
        /*[FieldOffset(588)]*/
        private long s24C;
        /*[FieldOffset(596)]*/
        private long s254;
        /// <summary>
        /// Placed type
        /// </summary>
        /*[FieldOffset(604)]*/
        public PlacedType PlacedType;
        /*[FieldOffset(608)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/
        private byte[] s260;
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 672;
            var st = new DealInternal();
            st.TicketNumber = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Id = GetString(buf.Bytes(64));
            st.Login = BitConverter.ToUInt64(buf.Bytes(8), 0);
            st.HistoryTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.OrderTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.s60 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.OpenTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            //st.s70 = new byte[8];
            //for (int i = 0; i < 8; i++)
            //	st.s70[i] = buf.Byte();
            buf.Bytes(8);
            st.Symbol = GetString(buf.Bytes(64));
            st.Type = (DealType)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.Direction = (Direction)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.OpenPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Price = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.StopLoss = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.TakeProfit = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
            st.Profit = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.ProfitRate = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.VolumeRate = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Commission = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Fee = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Swap = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.ExpertId = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.PositionTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.Comment = GetString(buf.Bytes(64));
            st.ContractSize = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.Digits = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.MoneyDigits = BitConverter.ToInt32(buf.Bytes(4), 0);
            //st.s178 = new byte[132];
            //for (int i = 0; i < 132; i++)
            //	st.s178[i] = buf.Byte();
            buf.Bytes(132);
            st.FreeProfit = BitConverter.ToDouble(buf.Bytes(8), 0);
            st.s204 = BitConverter.ToInt64(buf.Bytes(8), 0);
            //st.s20C = new byte[8];
            //for (int i = 0; i < 8; i++)
            //	st.s20C[i] = buf.Byte();
            buf.Bytes(8);
            st.TrailRounder = BitConverter.ToDouble(buf.Bytes(8), 0);
            //st.s21C = new byte[8];
            //for (int i = 0; i < 8; i++)
            //	st.s21C[i] = buf.Byte();
            buf.Bytes(8);
            st.OpenTimeMs = BitConverter.ToInt64(buf.Bytes(8), 0);
            //st.s22C = new byte[8];
            //for (int i = 0; i < 8; i++)
            //	st.s22C[i] = buf.Byte();
            buf.Bytes(8);
            st.s234 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.s23C = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.s244 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.s24C = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.s254 = BitConverter.ToInt64(buf.Bytes(8), 0);
            st.PlacedType = (PlacedType)BitConverter.ToInt32(buf.Bytes(4), 0);
            //st.s260 = new byte[64];
            //for (int i = 0; i < 64; i++)
            //	st.s260[i] = buf.Byte();
            buf.Bytes(64);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
            return st;
        }

        public bool AreEquals(DealInternal other)
        {
            if (other == null) return false;

            return
                TicketNumber == other.TicketNumber &&
                Id == other.Id &&
                Login == other.Login &&
                HistoryTime == other.HistoryTime &&
                OrderTicket == other.OrderTicket &&
                OpenTime == other.OpenTime &&
                Symbol == other.Symbol &&
                Type == other.Type &&
                Direction == other.Direction &&
                OpenPrice == other.OpenPrice &&
                Price == other.Price &&
                StopLoss == other.StopLoss &&
                TakeProfit == other.TakeProfit &&
                Volume == other.Volume &&
                Profit == other.Profit &&
                ProfitRate == other.ProfitRate &&
                VolumeRate == other.VolumeRate &&
                Commission == other.Commission &&
                Fee == other.Fee &&
                Swap == other.Swap &&
                ExpertId == other.ExpertId &&
                PositionTicket == other.PositionTicket &&
                Comment == other.Comment &&
                ContractSize == other.ContractSize &&
                Digits == other.Digits &&
                MoneyDigits == other.MoneyDigits &&
                FreeProfit == other.FreeProfit &&
                TrailRounder == other.TrailRounder &&
                OpenTimeMs == other.OpenTimeMs;
        }
    }
}
