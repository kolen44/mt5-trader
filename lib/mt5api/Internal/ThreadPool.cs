using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class ThreadPool
    {
        public static void QueueUserWorkItem(WaitCallback callBack, object state, int timeout) 
        {
            //System.Threading.ThreadPool.QueueUserWorkItem(callBack, state);
            Task.Run(() =>
            {
                try
                {
                    callBack(state);
                }
                catch (Exception ex)
                {
                    new Logger("ThreadPool").warn(ex, null);
                }
            });
        }
    }
}
