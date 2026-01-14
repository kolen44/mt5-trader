using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{

    public enum CommissionType
    {
        vDefaultCommission = 0,
        vTurnMoneyCommission = 1,
        vTurnLotsCommission = 2,
        vDealsCommission = 3,
        vProfitCommission = 4
    }
    public enum CommissionPeriod
    {
        vPerDailyCommission = 0,
        vPerMonthlyCommission = 1,
        vPerInstantCommission = 2
    };

    public enum InstantCommissionType
    {
        vInOutDealsCommission = 0,
        vInDealsCommission = 1,
        vOutDealsCommission = 2
    };

    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x164, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// ServerName from servers.dat
    /// </summary>
    public class ComissionInfo : FromBufReader
    {
        internal static readonly int Size = 0x38C;
        public Comission[] Comissions;

        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
        public string s0;
        /*[FieldOffset(128)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
        public string s80;
        /*[FieldOffset(256)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
        public string GroupName;
        /*[FieldOffset(512)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/
        public byte[] s200;
        /*[FieldOffset(576)]*/
        public int s240;
        /*[FieldOffset(580)]*/
        public CommissionType Type;
        /*[FieldOffset(584)]*/
        public CommissionPeriod Period;
        /*[FieldOffset(588)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/
        public string MoneyCurrency;
        /*[FieldOffset(620)]*/
        public InstantCommissionType InstantType;
        /*[FieldOffset(624)]*/
        public int s270;
        /*[FieldOffset(628)]*/
        public int s274;
        /*[FieldOffset(632)]*/
        public int s278;
        /*[FieldOffset(636)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 272)]*/
        public byte[] s27C;



        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + Size;
            var st = new ComissionInfo();
            st.s0 = GetString(buf.Bytes(128));
            st.s80 = GetString(buf.Bytes(128));
            st.GroupName = GetString(buf.Bytes(256));
            st.s200 = buf.Bytes(64);
            st.s240 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.Type = (CommissionType)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.Period = (CommissionPeriod)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.MoneyCurrency = GetString(buf.Bytes(32));
            st.InstantType = (InstantCommissionType)BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s270 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s274 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s278 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s27C = buf.Bytes(272);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}