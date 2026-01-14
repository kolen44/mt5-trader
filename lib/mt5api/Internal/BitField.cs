using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5.Internal
{
    struct Date
    {
        [BitfieldLength(5)]
        public uint Day;
        [BitfieldLength(4)]
        public uint Month;
        [BitfieldLength(7)]
        public uint Year;

        public static ushort Convert(DateTime dt)
        {
            var d = new Date();
            d.Day = (uint)dt.Day;
            d.Month = (uint)dt.Month;
            d.Year = (uint)dt.Year;
            return (ushort)PrimitiveConversion.ToLong(d);
        }

        public static ushort Convert(int day, int month, int year)
        {
            var d = new Date();
            d.Day = (uint)day;
            d.Month = (uint)month;
            d.Year = (uint)year;
            var res = (ushort)PrimitiveConversion.ToLong(d);
            return res;
        }
    };


    [global::System.AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class BitfieldLengthAttribute : Attribute
    {
        uint length;

        public BitfieldLengthAttribute(uint length)
        {
            this.length = length;
        }

        public uint Length { get { return length; } }
    }

    static class PrimitiveConversion
    {
        public static long ToLong<T>(T t) where T : struct
        {
            long r = 0;
            int offset = 0;

            // For every field suitably attributed with a BitfieldLength
            foreach (System.Reflection.FieldInfo f in t.GetType().GetFields())
            {
                object[] attrs = f.GetCustomAttributes(typeof(BitfieldLengthAttribute), false);
                if (attrs.Length == 1)
                {
                    uint fieldLength = ((BitfieldLengthAttribute)attrs[0]).Length;

                    // Calculate a bitmask of the desired length
                    long mask = 0;
                    for (int i = 0; i < fieldLength; i++)
                        mask |= (long)1 << i; 

                    r |= ((UInt32)f.GetValue(t) & mask) << offset;

                    offset += (int)fieldLength;
                }
            }

            return r;
        }
    }
}
