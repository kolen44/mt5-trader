using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
    class InBufAccLoad : InBuf
    {
        readonly ConcurrentQueue<byte[]> BufferQueue = new ConcurrentQueue<byte[]>();
        internal readonly AutoResetEvent AbbBufEvent = new AutoResetEvent(false);
        readonly int Timeout;
        public bool LoadFinish = false;
        int IndexForAllBufferrs = 0;

        public InBufAccLoad(byte[] buf, int start, int timeout) : base(buf, start)
        {
            Timeout = timeout;
        }

        public InBufAccLoad(byte[] buf, PacketHdr hdr, int timeout) : base(buf, hdr)
        {
            Timeout = timeout;
        }

        public void AddBuffer(byte[] buf)
        {
            BufferQueue.Enqueue(buf);
            AbbBufEvent.Set();
        }

        public override int CurrentIndex => IndexForAllBufferrs;

        public override byte Byte()
        {
            if (Ind == Buf.Length)
            {
                if (BufferQueue.TryDequeue(out var buf))
                {
                    Buf = buf;
                    Ind = 0;
                }
                else
                {
                    while (true)
                        if (AbbBufEvent.WaitOne(Timeout))
                        {
                            if (BufferQueue.TryDequeue(out var res))
                            {
                                Buf = res;
                                Ind = 0;
                                break;
                            }
                        }
                        else
                            throw new TimeoutException("Cannot get InBufAccLoad data in " + Timeout + "ms");
                }
            }
            IndexForAllBufferrs++;
            return base.Byte();
        }

        public override bool hasData
        {
            get
            {
                if (!LoadFinish)
                    return true;
                if(Ind >= Buf.Length && BufferQueue.Count == 0)
                    return false;
                else
                    return true;
            }
        }
        public override int Left => int.MaxValue;
    }
}
