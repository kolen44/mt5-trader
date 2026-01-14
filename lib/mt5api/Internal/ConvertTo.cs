using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    public class ConvertTo
    {
		public static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static DateTime DateTime(long time)
        {
            return StartTime.AddSeconds(time);
        }

        public static DateTime DateTimeMs(long time)
        {
            return StartTime.AddMilliseconds(time);
        }


        public static long Long(DateTime time)
		{
			if (time <= StartTime)
				return 0;
			return (long)time.Subtract(StartTime).TotalSeconds;
		}

        public static long LongMs(DateTime time)
        {
            if (time <= StartTime)
                return 0;
            return (long)time.Subtract(StartTime).TotalMilliseconds;
        }

        internal static double LongLongToDouble(int digits, long value)
        {
            digits = Math.Min(digits, 11);
            return Math.Round((double)(value) / DegreeP[digits], digits);
        }
        static double[] DegreeP = { 1.0, 1.0e1, 1.0e2, 1.0e3, 1.0e4, 1.0e5, 1.0e6, 1.0e7, 1.0e8, 1.0e9, 1.0e10, 1.0e11, 1.0e12, 1.0e13, 1.0e14, 1.0e15 };

		internal static string String(byte[] buf)
		{
			int count = 0;
			for (int i = 0; i < buf.Length; i += 2)
			{
				if (buf[i] == 0 && buf[i + 1] == 0)
					break;
				count++;
			}
			byte[] res = new byte[count * 2];
			for (int i = 0; i < count * 2; i++)
				res[i] = buf[i];
			string result = Encoding.Unicode.GetString(res);
			return result;
		}
	}
}
