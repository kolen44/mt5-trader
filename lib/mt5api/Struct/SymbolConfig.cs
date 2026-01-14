using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5.Struct
{
    public enum UpdateAction
    {
        /// <summary>
        /// Add symbol
        /// </summary>
        Add = 0,
        /// <summary>
        /// Update symbol
        /// </summary>
        Update = 1,
        /// <summary>
        /// Delete symbol
        /// </summary>
        Delete = 2
    }

    public class SymbolUpdate
    {
        public string Symbol { get; set; }
        public SymGroup Group { get; set; }
        public SymbolSessions Sessions { get; set; }
        internal SymTicks Ticks { get; set; }
        public UpdateAction Action { get; set; }
        public SymbolUpdate(string symbol, SymGroup group, SymbolSessions sessions, UpdateAction action)
        {
            Symbol = symbol;
            Group = group;
            Sessions = sessions;
            Action = action;
        }
    }


internal class SymbolConfig : FromBufReader                //sizeof 0x20 l
{
    internal static readonly int Size = 32;

    public int s0;
    public UpdateAction Action;              //4
    public int Number;              //8
    public int Index;               //C
    public int s10;
    public int s14;
    public int s18;
    public int s1C;

    internal override object ReadFromBuf(InBuf buf)
    {
        var endInd = buf.CurrentIndex + Size;
        var st = new SymbolConfig();
        st.s0 = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.Action = (UpdateAction)BitConverter.ToInt32(buf.Bytes(4), 0);
        st.Number = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.Index = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.s10 = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.s14 = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.s18 = BitConverter.ToInt32(buf.Bytes(4), 0);
        st.s1C = BitConverter.ToInt32(buf.Bytes(4), 0);
        if (buf.CurrentIndex != endInd)
            throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
        return st;
    }
}
}
