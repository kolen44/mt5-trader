using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    public static class ConcurrentBagHelper
    {
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }
    }
}
