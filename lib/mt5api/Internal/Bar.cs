using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// OHLC
    /// </summary>
    public class Bar //sizeof 0x3C d
    {
        public DateTime Time; //0
        public double OpenPrice; //8
        public double HighPrice; //10
        public double LowPrice; //18
        public double ClosePrice; //20
        public ulong TickVolume; //28
        public int Spread; //30
        public ulong Volume; //34

        public override string ToString()
        {
            return Time.ToString() + " " + OpenPrice;
        }
    }



    internal struct BarRecord          //sizeof 0x30 d          
    {
        public long Time;           //0
        public long OpenPrice;      //8
        public int High;            //10
        public int Low;             //14
        public int Close;           //18
        public int Spread;          //1C
        public ulong TickVolume;        //20
        public ulong Volume;            //28
    };

}
