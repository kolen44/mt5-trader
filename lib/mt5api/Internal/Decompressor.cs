using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mtapi.mt5
{
	internal class Decompressor
	{
		public unsafe static byte[] Decompress(byte[] src, int maxSize)
		{
			int dLen = maxSize;
            byte[] data;
			byte[] dstExt = new byte[dLen + 0x800];
			fixed (byte* s = src, d = dstExt)
			{
				if (DecompressData(s, src.Length, d, &dLen) == 0)
					throw new System.Exception();
                data = new byte[dLen];
                for (int i = 0; i < dLen; i++)
					data[i] = d[i];
			}
			Array.Resize(ref dstExt, 0);	
            return data;
		}

		unsafe static int DecompressData(byte* pSrc, int szSrc, byte* pDst, int* pszDst)
		{
			if ((pSrc == null) || (szSrc < 3) || (pDst == null) || (pszDst == null) || (*pszDst < 1))
				return 0;
			byte* pTo = pDst;
			byte* pEndSrc = &pSrc[szSrc];
			byte* pEndDst = &pDst[*pszDst];
			*pszDst = 0;
			if ((pEndSrc[-1] != 0) || (pEndSrc[-2] != 0) || (pEndSrc[-3] != 0x11))
				return 0;
			uint ch = *pSrc;
			if (ch > 0x11)
			{
				ch -= 0x11;
				pSrc++;
				if (ch < 4)
					goto LABEL_37;
				if ((pEndDst - pDst < ch) || (pEndSrc - pSrc < ch + 1))
					return 0;
				while (ch-- > 0)
					*pDst++ = *pSrc++;
				goto LABEL_31;
			}
		LABEL_15:
			ch = *pSrc++;
			if (ch >= 0x10)
				goto LABEL_41;
			if (ch == 0)
			{
				if (pSrc > pEndSrc)
					return 0;
				while (*pSrc == 0)
				{
					ch += 0xFF;
					if (++pSrc > pEndSrc)
						return 0;
				}
				ch += (uint)*pSrc++ + 0xF;
			}
			if ((pEndDst - pDst < ch + 3) || (pEndSrc - pSrc < ch + 4))
				return 0;
			*(uint*)pDst = *(uint*)pSrc;
			pDst += 4;
			pSrc += 4;
			if (--ch != 0)
			{
				while (ch >= 4)
				{
					*(uint*)pDst = *(uint*)pSrc;
					pDst += 4;
					pSrc += 4;
					ch -= 4;
				}
			}
			while (ch-- > 0)
				*pDst++ = *pSrc++;
		LABEL_31:
			ch = *pSrc++;
			if (ch >= 0x10)
				goto LABEL_41;
			byte* p = pDst - (*pSrc++ * 4) - (ch / 4) - 0x801;
			if ((p < pTo) || (pEndDst - pDst < 3))
				return 0;
			*pDst++ = *p++;
			*pDst++ = *p++;
			*pDst++ = *p++;
		LABEL_36:
			ch = (uint)pSrc[-2] & 3;
			if (ch == 0)
				goto LABEL_15;
		LABEL_37:
			if ((pEndDst - pDst < ch) || (pEndSrc - pSrc < ch + 1))
				return 0;
			while (ch-- > 0)
				*pDst++ = *pSrc++;
			ch = *pSrc++;
		LABEL_41:
			if (ch >= 0x40)
			{
				p = pDst - (*pSrc++ * 8) - ((ch / 4) & 7) - 1;
				ch = ch / 32 - 1;
				if ((p < pTo) || (pEndDst - pDst < ch + 2))
					return 0;
				*pDst++ = *p++;
				*pDst++ = *p++;
				while (ch-- > 0)
					*pDst++ = *p++;
				goto LABEL_36;
			}
			if (ch >= 0x20)
			{
				ch &= 0x1F;
				if (ch == 0)
				{
					if (pSrc > pEndSrc)
						return 0;
					while (*pSrc == 0)
					{
						ch += 0xFF;
						if (++pSrc > pEndSrc)
							return 0;
					}
					ch += (uint)*pSrc++ + 0x1F;
				}
				p = pDst - (*(ushort*)pSrc / 4) - 1;
				pSrc += 2;
				if ((p < pTo) || (pEndDst - pDst < ch + 2))
					return 0;
				if ((ch >= 6) && (pDst - p >= 4))
				{
					*(uint*)pDst = *(uint*)p;
					pDst += 4;
					p += 4;
					ch -= 2;
					while (ch >= 4)
					{
						*(uint*)pDst = *(uint*)p;
						pDst += 4;
						p += 4;
						ch -= 4;
					}
				}
				else
				{
					*pDst++ = *p++;
					*pDst++ = *p++;
				}
				while (ch-- > 0)
					*pDst++ = *p++;
				goto LABEL_36;
			}
			if (ch >= 0x10)
			{
				p = pDst - ((ch & 8) << 11);
				ch &= 7;
				if (ch == 0)
				{
					if (pSrc > pEndSrc)
						return 0;
					while (*pSrc == 0)
					{
						ch += 0xFF;
						if (++pSrc > pEndSrc)
							return 0;
					}
					ch += (uint)*pSrc++ + 7;
				}
				p -= (*(ushort*)pSrc / 4);
				pSrc += 2;
				if (p == pDst)
				{
					*pszDst = (int)(pDst - pTo);
					return (pSrc == pEndSrc) ? 1 : 0;
				}
				p -= 0x4000;
				if ((p < pTo) || (pEndDst - pDst < ch + 2))
					return 0;
				if ((ch >= 6) && (pDst - p >= 4))
				{
					*(uint*)pDst = *(uint*)p;
					pDst += 4;
					p += 4;
					ch -= 2;
					while (ch >= 4)
					{
						*(uint*)pDst = *(uint*)p;
						pDst += 4;
						p += 4;
						ch -= 4;
					}
				}
				else
				{
					*pDst++ = *p++;
					*pDst++ = *p++;
				}
				while (ch-- > 0)
					*pDst++ = *p++;
				goto LABEL_36;
			}
			p = pDst - (*pSrc++ * 4) - (ch / 4) - 1;
			*pDst++ = *p++;
			*pDst++ = *p++;
			if ((p >= pTo) && (pEndDst - pDst >= 2))
				goto LABEL_36;
			return 0;
		}

        public unsafe static byte[] Compress(byte[] src)
        {
            int sLen = src.Length;
            byte[] dstBuf = new byte[sLen];
            int dLen = 0;
            fixed (byte* s = src, d = dstBuf)
            {
                byte* p = d;
                if (sLen > 0xD)
                {
                    sLen = CompressData(s, sLen, d, &dLen, dstBuf);
                    p = d + dLen;
                }
                if (sLen > 0)
                {
                    byte* pFrom = s + src.Length - sLen;
                    if ((p == d) && (sLen <= 0xEE))
                        *p++ = (byte)(sLen + 0x11);
                    else if (sLen <= 3)
                        p[-2] |= (byte)sLen;
                    else if (sLen <= 0x12)
                        *p++ = (byte)(sLen - 3);
                    else
                    {
                        p++;
                        int cnt = sLen - 0x12;
                        if (cnt > 0xFF)
                        {
                            ulong lbl = 0x80808081 * (ulong)(cnt - 1);
                            int bl = (int)(lbl >> 39);
                            p += bl;
                            for (int i = 0; i < bl; i++)
                                cnt -= 0xFF;
                        }
                        *p++ = (byte)cnt;
                    }
                    for (int i = 0; i < sLen; i++)
                        *p++ = *pFrom++;
                }
                *p++ = 0x11;
                p += 2;
                dLen = (int)(p - d);
            }
            byte[] data = new byte[dLen];
            for (int i = 0; i < dLen; i++)
                data[i] = dstBuf[i];
            return data;
        }

        unsafe static int CompressData(byte* pSrc, int szSrc, byte* pDst, int* pszDst, byte[] dstBuf)
        {
            byte*[] PtrBuf = new byte*[0x4000];
            byte* pTo = pDst;
            byte* pFrom = pSrc + 4;
            byte* pStart = pSrc;
            byte* pEnd = pSrc + szSrc;
            while (true)
            {
                int index = (((((pFrom[3] << 6) ^ pFrom[2]) << 5) ^ pFrom[1]) << 5) ^ pFrom[0];
                index += index << 5;
                index = (index >> 5) & 0x3FFF;
                byte* pPtr = PtrBuf[index];
                if (pPtr < pSrc)
                    goto nextStep;
                szSrc = (int)(pFrom - pPtr);
                if ((szSrc == 0) || (szSrc > 0xBFFF))
                    goto nextStep;
                if ((szSrc > 0x800) && (pPtr[3] != pFrom[3]))
                {
                    index &= 0x7FF;
                    index ^= 0x201F;
                    pPtr = PtrBuf[index];
                    if (pPtr < pSrc)
                        goto nextStep;
                    szSrc = (int)(pFrom - pPtr);
                    if ((szSrc == 0) || (szSrc > 0xBFFF) || ((szSrc > 0x800) && (pPtr[3] != pFrom[3])))
                        goto nextStep;
                }
                if ((pPtr[0] != pFrom[0]) || (pPtr[1] != pFrom[1]) || (pPtr[2] != pFrom[2]))
                    goto nextStep;
                PtrBuf[index] = pFrom;
                int look = (int)(pFrom - pStart);
                if (look != 0)
                {
					if (look <= 3)
						pTo[-2] |= (byte)look;
					else if (look <= 0x12)
						*pTo++ = (byte)(look - 3);
					else
					{
						*pTo++ = 0;
						int cnt = look - 0x12;
						if (cnt > 0xFF)
						{
							ulong lbl = 0x80808081 * (ulong)(cnt - 1);
							int bl = (int)(lbl >> 39);
							pTo += bl;
							for (int i = 0; i < bl; i++)
								cnt -= 0xFF;
						}
						*pTo++ = (byte)cnt;
					}
                    for (int i = 0; i < look; i++)
                        *pTo++ = *pStart++;
                }
                pFrom += 3;
                if ((*pFrom++ == pPtr[3]) && (*pFrom++ == pPtr[4]) && (*pFrom++ == pPtr[5]) &&
                    (*pFrom++ == pPtr[6]) && (*pFrom++ == pPtr[7]) && (*pFrom++ == pPtr[8]))
                {
                    pPtr += 9;
                    while (pFrom < pEnd)
                    {
                        if (*pPtr != *pFrom)
                            break;
                        pPtr++;
                        pFrom++;
                    }
                    look = (int)(pFrom - pStart);
                    if (szSrc <= 0x4000)
                    {
                        szSrc--;
                        if (look <= 0x21)
                            *pTo++ = (byte)((look - 2) | 0x20);
                        else
                        {
                            *pTo++ = 0x20;
                            int cnt = look - 0x21;
                            if (cnt > 0xFF)
                            {
                                ulong lbl = 0x80808081 * (ulong)(cnt - 1);
                                int bl = (int)(lbl >> 39);
                                pTo += bl;
                                for (int i = 0; i < bl; i++)
                                    cnt -= 0xFF;
                            }
                            *pTo++ = (byte)cnt;
                        }
                    }
                    else
                    {
                        szSrc -= 0x4000;
                        if (look <= 9)
                            *pTo++ = (byte)(((szSrc >> 11) & 8) | (look - 2) | 0x10);
                        else
                        {
                            *pTo++ = (byte)(((szSrc >> 11) & 8) | 0x10);
                            int cnt = look - 9;
                            if (cnt > 0xFF)
                            {
                                ulong lbl = 0x80808081 * (ulong)(cnt - 1);
                                int bl = (int)(lbl >> 39);
                                pTo += bl;
                                for (int i = 0; i < bl; i++)
                                    cnt -= 0xFF;
                            }
                            *pTo++ = (byte)cnt;
                        }
                    }
                    *pTo++ = (byte)(szSrc << 2);
                    *pTo++ = (byte)(szSrc >> 6);
                }
                else
                {
                    look = (int)(--pFrom - pStart);
                    if (szSrc <= 0x800)
                    {
                        szSrc--;
                        *pTo++ = (byte)(((look + 7) << 5) | ((szSrc & 7) << 2));
                        *pTo++ = (byte)(szSrc >> 3);
                    }
                    else if (szSrc <= 0x4000)
                    {
                        *pTo++ = (byte)((look - 2) | 0x20);
                        szSrc--;
                        *pTo++ = (byte)(szSrc << 2);
                        *pTo++ = (byte)(szSrc >> 6);
                    }
                    else
                    {
                        szSrc -= 0x4000;
                        *pTo++ = (byte)(((szSrc >> 11) & 8) | (look - 2) | 0x10);
                        *pTo++ = (byte)(szSrc << 2);
                        *pTo++ = (byte)(szSrc >> 6);
                    }
                }
                pStart = pFrom;
                goto checkEnd;
            nextStep:
                PtrBuf[index] = pFrom++;
            checkEnd:
                if (pFrom >= (pEnd - 0xD))
                    break;
            }
            *pszDst = (int)(pTo - pDst);
            return (int)(pEnd - pStart);
        }
    }
}
