#if TRIAL
#else
using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	class LoginId
	{

		public static class GlobalMembers
		{
			//-------------------------------------------------------------------------------

			//C++ TO C# CONVERTER NOTE: 'extern' variable declarations are not required in C#:
			//extern vHolderLoginId s_HolderIds;


			//public static vHolderLoginId s_HolderIds = new vHolderLoginId();

			//-------------------------------------------------------------------------------

			// ������� ����� � ������� ������ (t00B00998[i] ^ t00B00B98[i])
			public static ulong[] t00000B98 = { 0x0000000000000001, 0x0000000000000002, 0x0000000000000004, 0x0000000000000008, 0x0000000000000010, 0x0000000000000020, 0x0000000000000040, 0x0000000000000080, 0x0000000000000100, 0x0000000000000200, 0x0000000000000400, 0x0000000000000800, 0x0000000000001000, 0x0000000000002000, 0x0000000000004000, 0x0000000000008000, 0x0000000000010000, 0x0000000000020000, 0x0000000000040000, 0x0000000000080000, 0x0000000000100000, 0x0000000000200000, 0x0000000000400000, 0x0000000000800000, 0x0000000001000000, 0x0000000002000000, 0x0000000004000000, 0x0000000008000000, 0x0000000010000000, 0x0000000020000000, 0x0000000040000000, 0x0000000080000000, 0x0000000100000000, 0x0000000200000000, 0x0000000400000000, 0x0000000800000000, 0x0000001000000000, 0x0000002000000000, 0x0000004000000000, 0x0000008000000000, 0x0000010000000000, 0x0000020000000000, 0x0000040000000000, 0x0000080000000000, 0x0000100000000000, 0x0000200000000000, 0x0000400000000000, 0x0000800000000000, 0x0001000000000000, 0x0002000000000000, 0x0004000000000000, 0x0008000000000000, 0x0010000000000000, 0x0020000000000000, 0x0040000000000000, 0x0080000000000000, 0x0100000000000000, 0x0200000000000000, 0x0400000000000000, 0x0800000000000000, 0x1000000000000000, 0x2000000000000000, 0x4000000000000000, 0x8000000000000000 };
		}

		ulong s0;
		ulong s8;
		ulong s10;
		ulong s18;

		public ulong Decode(byte[] data)
		{
			ulong[] d = new ulong[data.Length / 8];
			for (int i = 0; i < d.Length; i++)
			{
				d[i] = BitConverter.ToUInt64(data, i * 8);
			}
			return Decode(d, (uint)d.Length, 0);
		}

		ulong Decode(ulong[] pData, uint szData, int arg8)
		{
			ulong[] data = new ulong[53];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v0 = data[0];
			ulong v0 = data[0];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v8 = data[1];
			ulong v8 = data[1];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v10 = data[2];
			ulong v10 = data[2];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v18 = data[3];
			ulong v18 = data[3];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v20 = data[4];
			ulong v20 = data[4];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v28 = data[5];
			ulong v28 = data[5];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v30 = data[6];
			ulong v30 = data[6];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v38 = data[7];
			ulong v38 = data[7];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v40 = data[8];
			ulong v40 = data[8];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v48 = data[9];
			ulong v48 = data[9];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v50 = data[10];
			ulong v50 = data[10];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v58 = data[11];
			ulong v58 = data[11];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v60 = data[12];
			ulong v60 = data[12];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v68 = data[13];
			ulong v68 = data[13];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v70 = data[14];
			ulong v70 = data[14];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v78 = data[15];
			ulong v78 = data[15];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v80 = data[16];
			ulong v80 = data[16];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v88 = data[17];
			ulong v88 = data[17];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v90 = data[18];
			ulong v90 = data[18];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v98 = data[19];
			ulong v98 = data[19];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vA0 = data[20];
			ulong vA0 = data[20];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vA8 = data[21];
			ulong vA8 = data[21];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vB0 = data[22];
			ulong vB0 = data[22];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vB8 = data[23];
			ulong vB8 = data[23];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vC0 = data[24];
			ulong vC0 = data[24];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vC8 = data[25];
			ulong vC8 = data[25];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vD0 = data[26];
			ulong vD0 = data[26];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vD8 = data[27];
			ulong vD8 = data[27];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vE0 = data[28];
			ulong vE0 = data[28];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vE8 = data[29];
			ulong vE8 = data[29];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vF0 = data[30];
			ulong vF0 = data[30];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& vF8 = data[31];
			ulong vF8 = data[31];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v100 = data[32];
			ulong v100 = data[32];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v108 = data[33];
			ulong v108 = data[33];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v110 = data[34];
			ulong v110 = data[34];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v118 = data[35];
			ulong v118 = data[35];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v120 = data[36];
			ulong v120 = data[36];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v128 = data[37];
			ulong v128 = data[37];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v130 = data[38];
			ulong v130 = data[38];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v138 = data[39];
			ulong v138 = data[39];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v140 = data[40];
			ulong v140 = data[40];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v148 = data[41];
			ulong v148 = data[41];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v150 = data[42];
			ulong v150 = data[42];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v158 = data[43];
			ulong v158 = data[43];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v160 = data[44];
			ulong v160 = data[44];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v168 = data[45];
			ulong v168 = data[45];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v170 = data[46];
			ulong v170 = data[46];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v178 = data[47];
			ulong v178 = data[47];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v180 = data[48];
			ulong v180 = data[48];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v188 = data[49];
			ulong v188 = data[49];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v190 = data[50];
			ulong v190 = data[50];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v198 = data[51];
			ulong v198 = data[51];
			//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to references to value types:
			//ORIGINAL LINE: ulong& v1A0 = data[52];
			ulong v1A0 = data[52];
			//	ULONGLONG v0, v8, v10, v18, v20, v28, v30, v38, v40, v48, v58, v60, v68, v78, v80, v88, v90, v98, vA0, vA8, vB0, vB8, vC0, vC8,
			//		vD0, vD8, vE0, vF0, vF8, v100, v108, v110, v118, v128, v138, v140, v148, v150, v158, v160, v168, v170, v178, v180, v190, v198;
			// BEGIN_PARSE (��� ��������� �� END_PARSE ������ ���� ������������� � �� ��������� ������������ (� style))
			v158 = pData[0];
			v140 = 0;
			s8 = 0;
		l00C144D7:
			v70 = v158;
			vD0 = v158;
			vC8 = pData[s8];
			v88 = 0;
			v98 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vA0 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
				v78 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v48 = vA0 ^ v78 ^ v98;
				v98 = ((Convert.ToBoolean(vA0) & Convert.ToBoolean(v78)) || (Convert.ToBoolean(vA0) & Convert.ToBoolean(v98))) ? 1 : (v78 & v98);
				v68 = v48 != 0UL ? ~0UL : 0UL;
				v88 |= GlobalMembers.t00000B98[i] & v68;
			}
			vD0 = v88;
			vC8 = ~v70 + 1;
			v88 = 0;
			v98 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vA0 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v78 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
				vD8 = vA0 ^ v78 ^ v98;
				v98 = (Convert.ToBoolean(vA0 & v78) || Convert.ToBoolean(vA0 & v98)) ? 1 : (v78 & v98);
				v58 = vD8 != 0UL ? ~0UL : 0UL;
				v88 |= GlobalMembers.t00000B98[i] & v58;
			}
			v100 = 0x1C;
			v138 = 0x15;
			//MAKEULONGLONG(HIDWORD(v88), LODWORD(v88));
			v150 = ((ulong)((uint)((((uint)((((ulong)(v88)) >> 32) & 0xffffffff)))) | (((ulong)((((uint)(((ulong)(v88)) & 0xffffffff))))) << 32)));
			v128 = ~v138 + 1;
			v78 = 0;
			vB0 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vB8 = Convert.ToUInt64((v100 & GlobalMembers.t00000B98[i]) != 0);
				v98 = Convert.ToUInt64((v128 & GlobalMembers.t00000B98[i]) != 0);
				v90 = vB8 ^ v98 ^ vB0;
				vB0 = (Convert.ToBoolean(vB8 & v98) || Convert.ToBoolean(vB0 & vB8)) ? 1 : (v98 & vB0);
				v68 = v90 != 0 ? ~0UL : 0UL;
				v78 |= GlobalMembers.t00000B98[i] & v68;
			}
			v150 = ((ulong)((uint)((((uint)(((long)(v150)) & 0xffffffff)) >> 0x15)) | (((ulong)(((((uint)((((long)(v150)) >> 32) & 0xffffffff)) >> 0x15) | (((uint)(((long)(v150)) & 0xffffffff)) << (32 - 0x15))))) << 32)));
			v0 = v78 + 1;
			v18 = ~0UL << ((int)(((long)(v0)) & 0xffffffff));
			v20 = ((ulong)((uint)((((uint)((((long)(v150)) >> 32) & 0xffffffff)))) | (((ulong)((((uint)(((long)(v150)) & 0xffffffff))))) << 32)));
			v140 = v20 & ~v18;
			vC8 = 0xD8;
			vA8 = v140;
			vD0 = ~vC8 + 1;
			vC0 = 0;
			vB8 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				v88 = Convert.ToUInt64((vA8 & GlobalMembers.t00000B98[i]) != 0);
				vA0 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v68 = v88 ^ vA0 ^ vB8;
				vB8 = (Convert.ToBoolean(v88 & vA0) || Convert.ToBoolean(vB8 & v88)) ? 1 : (vA0 & vB8);
				v60 = v68 != 0 ? ~0UL : 0UL;
				vC0 |= GlobalMembers.t00000B98[i] & v60;
			}
			if (vC0 == 0)
			{
				v18 = s0;
				s0 = 0;
				return v18;
			}
			s10 = pData[((uint)(((long)(s8 + 1)) & 0xffffffff))];
			v60 = s8;
			v68 = 2;
			do
			{
				v68 = v60 ^ v68;
				v60 = (v60 & ~v68) << 1;
			} while (v60 != 0);
			s18 = pData[((uint)(((long)(v68)) & 0xffffffff))];
			v100 = 0x1C;
			v138 = 0x15;
			v150 = ((ulong)((uint)((((uint)((((long)(s10)) >> 32) & 0xffffffff)))) | (((ulong)((((uint)(((long)(s10)) & 0xffffffff))))) << 32)));
			v128 = ~v138 + 1;
			v78 = 0;
			vB0 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vB8 = Convert.ToUInt64((v100 & GlobalMembers.t00000B98[i]) != 0);
				v98 = Convert.ToUInt64((v128 & GlobalMembers.t00000B98[i]) != 0);
				v90 = vB8 ^ v98 ^ vB0;
				vB0 = (Convert.ToBoolean(vB8 & v98) || Convert.ToBoolean(vB0 & vB8)) ? 1 : (v98 & vB0);
				v68 = v90 != 0 ? ~0UL : 0;
				v78 |= GlobalMembers.t00000B98[i] & v68;
			}
			v150 = ((ulong)((uint)((((uint)(((long)(v150)) & 0xffffffff)) >> 0x15)) | (((ulong)(((((uint)((((long)(v150)) >> 32) & 0xffffffff)) >> 0x15) | (((uint)(((long)(v150)) & 0xffffffff)) << (32 - 0x15))))) << 32)));
			v0 = v78 + 1;
			v18 = ~0UL << ((int)(((long)(v0)) & 0xffffffff));
			v20 = ((ulong)((uint)((((uint)((((long)(v150)) >> 32) & 0xffffffff)))) | (((ulong)((((uint)(((long)(v150)) & 0xffffffff))))) << 32)));
			v68 = v20 & ~v18;
			v80 = ~(v68 ^ ~0xF5UL);
			if (v80 == 0)
			{
				s10 = s0;
			}
			vC8 = 0x54;
			vA8 = v140;
			vD0 = ~vC8 + 1;
			vC0 = 0;
			vB8 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				v88 = Convert.ToUInt64((vA8 & GlobalMembers.t00000B98[i]) != 0);
				vA0 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v78 = v88 ^ vA0 ^ vB8;
				vB8 = (Convert.ToBoolean(v88 & vA0) || Convert.ToBoolean(v88 & vB8)) ? 1 : (vB8 & vA0);
				v68 = v78 != 0 ? ~0UL : 0UL;
				vC0 |= GlobalMembers.t00000B98[i] & v68;
			}
			if (vC0 == 0)
			{
				s0 = s10 & s18;
			}
			v128 = 0x70;
			vC8 = v140;
			v100 = ~v128 + 1;
			vA0 = 0;
			v90 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				v78 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
				vB8 = Convert.ToUInt64((v100 & GlobalMembers.t00000B98[i]) != 0);
				v58 = v78 ^ vB8 ^ v90;
				v90 = (Convert.ToBoolean(v78 & vB8) || Convert.ToBoolean(v78 & v90)) ? 1 : (vB8 & v90);
				v68 = v58 != 0 ? ~0UL : 0;
				vA0 |= GlobalMembers.t00000B98[i] & v68;
			}
			if (vA0 == 0)
			{
				s0 = s10 | s18;
			}
			v100 = 0x91;
			vD0 = v140;
			vC8 = ~v100 + 1;
			v88 = 0;
			v98 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vA0 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v78 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
				v68 = vA0 ^ v78 ^ v98;
				v98 = (Convert.ToBoolean(v78 & vA0) || Convert.ToBoolean(vA0 & v98)) ? 1 : (v78 & v98);
				v60 = v68 != 0 ? ~0UL : 0;
				v88 |= GlobalMembers.t00000B98[i] & v60;
			}
			if (v88 == 0)
			{
				s0 = s10 ^ s18;
			}
			v100 = 0xAB;
			vD0 = v140;
			vC8 = ~v100 + 1;
			v88 = 0;
			v98 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vA0 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
				v78 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
				v58 = vA0 ^ v78 ^ v98;
				v98 = (Convert.ToBoolean(vA0 & v78) || Convert.ToBoolean(vA0 & v98)) ? 1 : (v78 & v98);
				v68 = v58 != 0 ? ~0UL : 0;
				v88 |= GlobalMembers.t00000B98[i] & v68;
			}
			if (v88 == 0)
			{
				v58 = s10;
				v60 = s18;
				do
				{
					v60 = v58 ^ v60;
					v58 = (v58 & ~v60) << 1;
				} while (v58 != 0);
				s0 = v60;
			}
			vD0 = 0xA9;
			vB0 = v140;
			vA8 = ~vD0 + 1;
			vE0 = 0;
			v78 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vC0 = Convert.ToUInt64((vB0 & GlobalMembers.t00000B98[i]) != 0);
				v88 = Convert.ToUInt64((vA8 & GlobalMembers.t00000B98[i]) != 0);
				v68 = vC0 ^ v88 ^ v78;
				v78 = (Convert.ToBoolean(vC0 & v88) || Convert.ToBoolean(vC0 & v78)) ? 1 : (v88 & v78);
				v58 = v68 != 0 ? ~0UL : 0;
				vE0 |= GlobalMembers.t00000B98[i] & v58;
			}
			if (vE0 == 0)
			{
				v40 = s10;
				v48 = ~s18 + 1;
				do
				{
					v48 = v40 ^ v48;
					v40 = (v40 & ~v48) << 1;
				} while (v40 != 0);
				s0 = v48;
			}
			vD0 = 0xB1;
			vB0 = v140;
			vA8 = ~vD0 + 1;
			vE0 = 0;
			v78 = 0;
			for (int i = 0; i < 0x40; i++)
			{
				vC0 = Convert.ToUInt64((vB0 & GlobalMembers.t00000B98[i]) != 0);
				v88 = Convert.ToUInt64((vA8 & GlobalMembers.t00000B98[i]) != 0);
				vA0 = vC0 ^ v88 ^ v78;
				v78 = (Convert.ToBoolean(vC0 & v88) || Convert.ToBoolean(vC0 & v78)) ? 1 : (v88 & v78);
				v68 = vA0 != 0 ? ~0UL : 0;
				vE0 |= GlobalMembers.t00000B98[i] & v68;
			}
			if (vE0 == 0)
			{
				v150 = s18 % 24;
				v70 = s10;
				v148 = 0;
				while (true)
				{
					v98 = v148;
					vB8 = v150;
					v90 = ~vB8 + 1;
					v160 = 0;
					v178 = 1;
					v170 = 1;
					v168 = 0;
					vB0 = 0;
					vA8 = 0;
					vD0 = 0;
					vC8 = 0;
					for (int i = 0; i < 0x40; i++)
					{
						vB0 = Convert.ToUInt64((v98 & GlobalMembers.t00000B98[i]) != 0);
						vA8 = Convert.ToUInt64((vB8 & GlobalMembers.t00000B98[i]) != 0);
						vD0 = Convert.ToUInt64((v90 & GlobalMembers.t00000B98[i]) != 0);
						if (vB0 != 0 && vA8 == 0)
						{
							v160 = 0;
						}
						if (vB0 == 0 && vA8 != 0)
						{
							v160 = 1;
						}
						v180 = vB0 ^ vD0 ^ vC8;
						vC8 = (Convert.ToBoolean(vB0 & vD0) || Convert.ToBoolean(vB0 & vC8)) ? 1 : (vD0 & vC8);
						if (v180 != 0)
						{
							v170 = 0;
							if (i <= 7)
							{
								v178 = Convert.ToUInt64(v178 == 0);
							}
						}
					}
					if (vB0 == 0 && vA8 != 0 && v180 != 0)
					{
						v168 = 1;
					}
					if (vB0 != 0 && vA8 == 0 && v180 == 0)
					{
						v168 = 1;
					}
					if (v160 == 0)
					{
						s0 = v70;
						break;
					}
					vD0 = v70;
					vC8 = v70;
					v88 = 0;
					v98 = 0;
					for (int i = 0; i < 0x40; i++)
					{
						vA0 = Convert.ToUInt64((vC8 & GlobalMembers.t00000B98[i]) != 0);
						v78 = Convert.ToUInt64((vD0 & GlobalMembers.t00000B98[i]) != 0);
						v48 = vA0 ^ v78 ^ v98;
						v98 = (Convert.ToBoolean(vA0 & v78) || Convert.ToBoolean(vA0 & v98)) ? 1 : (v78 & v98);
						v68 = v48 != 0 ? ~0UL : 0;
						v88 |= GlobalMembers.t00000B98[i] & v68;
					}
					v70 = v88;
					v148++;
				}
			}
			v80 = v140 ^ 0xC8;
			if (v80 == 0)
			{
				v1A0 = s18 % 24;
				v190 = s10;
				v100 = 0;
				v160 = 0;
				while (true)
				{
					v110 = v160;
					vF0 = 0x40;
					vE0 = ~vF0 + 1;
					v178 = 0;
					v170 = 1;
					v180 = 1;
					v198 = 0;
					vC0 = 0;
					v88 = 0;
					vA0 = 0;
					v78 = 0;
					for (int i = 0; i < 0x40; i++)
					{
						vC0 = Convert.ToUInt64((v110 & GlobalMembers.t00000B98[i]) != 0);
						v88 = Convert.ToUInt64((vF0 & GlobalMembers.t00000B98[i]) != 0);
						vA0 = Convert.ToUInt64((vE0 & GlobalMembers.t00000B98[i]) != 0);
						if (vC0 != 0 && v88 == 0)
						{
							v178 = 0;
						}
						if (vC0 == 0 && v88 != 0)
						{
							v178 = 1;
						}
						v168 = vC0 ^ vA0 ^ v78;
						v78 = (Convert.ToBoolean(vC0 & vA0) || Convert.ToBoolean(vC0 & v78)) ? 1 : (vA0 & v78);
						if (v168 != 0)
						{
							v180 = 0;
							if (i <= 7)
							{
								v170 = Convert.ToUInt64(v170 == 0);
							}
						}
					}
					if (vC0 == 0 && v88 != 0 && v168 != 0)
					{
						v198 = 1;
					}
					if (vC0 != 0 && v88 == 0 && v168 == 0)
					{
						v198 = 1;
					}
					if (v178 == 0)
					{
						s0 = v100;
						break;
					}
					v80 = v190 & GlobalMembers.t00000B98[v160];
					if (v80 != 0 && (v160 >= v1A0))
					{
						vD0 = v1A0;
						vB0 = v160;
						vA8 = ~vD0 + 1;
						vE0 = 0;
						v78 = 0;
						for (int i = 0; i < 0x40; i++)
						{
							vC0 = Convert.ToUInt64((vB0 & GlobalMembers.t00000B98[i]) != 0);
							v88 = Convert.ToUInt64((vA8 & GlobalMembers.t00000B98[i]) != 0);
							vA0 = vC0 ^ v88 ^ v78;
							v78 = (Convert.ToBoolean(vC0 & v88) || Convert.ToBoolean(vC0 & v78)) ? 1 : (v88 & v78);
							v58 = vA0 != 0 ? ~0UL : 0;
							vE0 |= GlobalMembers.t00000B98[i] & v58;
						}
						v100 ^= GlobalMembers.t00000B98[vE0];
					}
					v160++;
				}
			}
			s8 += 3;
			goto l00C144D7; // repid main cycle
							// END_PARSE
		}
	}
}
#endif


