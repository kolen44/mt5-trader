using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
	public partial class MT5API
	{

		static public AccountAnswer RequestDemoAccount(AccountRequest req, string host, int port)
		{
			req.Random = (byte)DateTime.Now.Ticks;
			req.Type = 0;
			req.Revision = (short)4885;
			req.Signature = 20813;
			req.Time = (int)ConvertTo.Long(DateTime.Now);
			req.Key = new byte[] {
					0x64, 0x3c, 0x43, 0x57,
					0x33, 0x9d, 0xe8, 0xbe,
					0x19, 0xd8, 0x0d, 0x09,
					0x8a, 0xaa, 0x3b, 0xa2
				};
			req.RanddomTail = new byte[32];
			var buf = new OutBuf();
			buf.Add(req.Random);
			buf.Add(req.Type);
			buf.Add(req.Revision);
			buf.Add(req.Signature);
			buf.Add(req.Key);
			buf.Add(req.Time);
			buf.Add(req.UserName, 64);
			buf.Add(req.AccType, 32);
			buf.Add(req.s19A, 16);
			buf.Add(req.Country, 16);
			buf.Add(req.City, 16);
			buf.Add(req.State, 16);
			buf.Add(req.ZipCode, 8);
			buf.Add(req.Address, 64);
			buf.Add(req.Phone, 16);
			buf.Add(req.Email, 32);
			buf.Add(req.CompanyName, 32);
			buf.Add(req.Deposit);
			buf.Add(req.Leverage);
			buf.Add(req.LanguageId);
			buf.Add(req.UtmCampaign, 16);
			buf.Add(req.Flags);
			buf.Add(req.PushID);
			buf.Add(req.NetAddr, 20);
			buf.Add(req.AgreeFlags);
			buf.Add(req.RanddomTail, 32);
			var ebuf = Crypt.EasyCrypt(buf.List.ToArray());
			var ob = new OutBuf(ebuf);
			ob.CreateHeader(4, 0, false);
			var api = new MT5API(0, "", host, port);
			Connection con = new Connection(api);
            var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds((double)api.ConnectTimeout)).Token;
            con.Connect(cancellation).Wait();
			con.Send(ob.List.ToArray()).Wait();
			var bytes = con.Receive(9).Result;
			var hdr = UDT.ReadStruct<PacketHdr>(bytes, 0, 9);
			if (hdr.Type != 4)
				throw new Exception("hdr.Type != 4");
			bytes = con.Receive(hdr.PacketSize).Result;
			var dbytes = Crypt.EasyDecrypt(bytes);
			InBuf ib = new InBuf(dbytes, 0);
			var acc = new AccountAnswer
			{
				s0 = ib.Int(),
				Status = (Msg)ib.Int(),
				Login = ib.ULong(),
				Password = ConvertTo.String(ib.Bytes(32)),
				Investor = ConvertTo.String(ib.Bytes(32))
			};
			if (acc.Status != Msg.DONE)
				throw new Exception(acc.Status.ToString());
			return acc;
		}
	}

	public class AccountAnswer
	{
		internal int s0;
		internal Msg Status;
		public ulong Login;
		public string Password;
		public string Investor;
	}
}

/*
 *vAccRequest req;
	req = *m_pAccReq;
	req.m_cRandom = (char)GetTickCount();
	req.m_Type = 0;
	req.m_nRevision = CLIENT_BUILD;				//MT5 Terminal build
	req.m_Signature = 'QM';						//MetaQuotes
	req.m_nTime = (LONG)_time64(NULL);
	vCrypt::GetCommonKey(req.m_Key);
	vCrypt::RandomizationEnd<128>(req.m_sUserName);
	vCrypt::RandomizationEnd<64>(req.m_sAccType);
	vCrypt::RandomizationEnd<32>(req.s19A);
	vCrypt::RandomizationEnd<32>(req.m_sCountry);
	vCrypt::RandomizationEnd<32>(req.m_sCity);
	vCrypt::RandomizationEnd<32>(req.m_sState);
	vCrypt::RandomizationEnd<16>(req.m_sZipCode);
	vCrypt::RandomizationEnd<128>(req.m_sAddress);
	vCrypt::RandomizationEnd<32>(req.m_sPhone);
	vCrypt::RandomizationEnd<64>(req.m_sEmail);
	vCrypt::RandomizationEnd<64>(req.m_sCompanyName);
	vCrypt::Randomization(req.s54E, 60);
	vCrypt::Encrypt(&req, sizeof(vAccRequest), NULL, 0);
	vSockBufManager bufMan(0);
	bufMan.SetBufferSize(sizeof(vAccRequest));
	bufMan.Clear();
	bufMan.DataToBuffer(&req, sizeof(vAccRequest));
	WipeBuffer(&req, sizeof(vAccRequest));
	bufMan.CreatePacketHeader(4, GetSendPacketId());
	if (await Receive())
		SendBuffer(&bufMan);
*/
