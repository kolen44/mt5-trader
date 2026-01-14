using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    public enum vCommissionUnitType
    {
        vCurrencyUnit = 0,
        vBaseCurrencyUnit = 1,
        vProfitCurrencyUnit = 2,
        vMarginCurrencyUnit = 3,
        vPointsUnit = 4,
        vMoneyCurrencyUnit = 5,
        vSpecifiedUnit = 6,
        vProfitUnit = 7
    }

    public enum vCommissionUnitMode
    {
        vPerDealCommission = 0,
        vPerLotCommission = 1,
        vPerValueCommission = 2
    }
    public class Comission : FromBufReader
    {
        internal static readonly int Size = 0xA0;
        //[FieldOffset(0)] 
        public vCommissionUnitType UnitType;
        //[FieldOffset(4)]
        public vCommissionUnitMode UnitMode;
        //[FieldOffset(8)] 
        public double UnitValue;
        //[FieldOffset(16)] 
        public double MinValue;
        //[FieldOffset(24)] 
        public double MaxValue;
        ///[FieldOffset(32)] 
        public double MinUnit;
        //[FieldOffset(40)][MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)] 
        public string CurrencyUnit;
        //[FieldOffset(72)] 
        public double MaxUnit;
        //[FieldOffset(80)][MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)] 
        public byte[] s50;

        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 0xA0;
            var st = new Comission();
            st.UnitType = (vCommissionUnitType)buf.Int();
            st.UnitMode = (vCommissionUnitMode)buf.Int();
            st.UnitValue = buf.Double();
            st.MinValue = buf.Double();
            st.MaxValue = buf.Double();
            st.MinUnit = buf.Double();
            st.CurrencyUnit = GetString(buf.Bytes(32));
            st.MaxUnit = buf.Double();
            st.s50 = buf.Bytes(80);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}
