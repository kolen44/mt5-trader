using mtapi.mt5.Internal;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class Connection : SecureSocket
    {
#if TRIAL
		/// <summary>
		/// Calculate LoginId on WebServer instead of local LoginId.dll
		/// </summary>
		string TrialLoginIdPath = "http://trial.mtapi.io";
#endif

		internal readonly MT5API QC;
		internal readonly ExLogin ExLogin = null;

		public Connection(MT5API qc)
        {
            QC = qc;
			if (QC.ExLoginManagers != null || QC.ExLoginProxies != null)
				ExLogin = new ExLogin(QC.ExLoginManagers, QC.ExLoginProxies, qc);
		}

        public Connection(Connection con)
        {
            QC = con.QC;
			if (QC.ExLoginManagers != null || QC.ExLoginProxies != null)
				ExLogin = new ExLogin(QC.ExLoginManagers, QC.ExLoginProxies, QC);
		}

        public async Task Login(bool bDataCenter, Logger log, ConnectLogs process, CancellationToken cancellation)
        {
            log.trace("Connecting to server");
            if (QC.Host == "108.181.97.196")
                CLIENT_BUILD = 3900;
            if(QC.Build !=0 )
                CLIENT_BUILD = (ulong)QC.Build;
			try
            {
				if (QC.ProxyEnable)
				{
                    process.Progress += ", connecting to server " + QC.Host + ":" + QC.Port + " proxy  " + " " + QC.ProxyHost + ":" + QC.ProxyPort + " " + QC.ProxyType;
                    await Connect(QC.Host, QC.Port, QC.ProxyHost, QC.ProxyPort, QC.ProxyUser, QC.ProxyPassword, QC.ProxyType, cancellation);
					process.Progress += ", proxy socket opened " + QC.Host + ":" + QC.Port + " proxy  " + " " + QC.ProxyHost + ":" + QC.ProxyPort + " " + QC.ProxyType;
					log.trace("Proxy socket opened " + QC.Host + ":" + QC.Port + " proxy  " + " " + QC.ProxyHost + ":" + QC.ProxyPort + " " + QC.ProxyType);
				}
				else
				{
                    process.Progress += $", connecting to server {QC.Host}:{QC.Port}";
                    await Connect(QC.Host, QC.Port, cancellation);
					process.Progress += ", socket opened " + QC.Host + ":" + QC.Port;
					log.trace("Socket opened " + QC.Host + ":" + QC.Port);
				}
                await SendLogin(log, process);
            }
            catch (Exception ex)
            {
                log.trace(ex.ToString());
                Close();
                throw;
            }
        }

        private readonly SemaphoreSlim SendLock = new SemaphoreSlim(1, 1);
        internal async Task SendPacket(byte type, OutBuf buf, bool compressed = false)
        {
            await SendLock.WaitAsync();
            try
            {
                buf.CreateHeader(type, GetSendId(), compressed);
                Encryptor.EncryptPacket(buf);
                await Send(buf.ToArray());
                buf.List.Clear();
            }
            finally
            {
                SendLock.Release();
            }
        }

        internal async Task SendCompress(byte type, OutBuf buf)
        {
            var compr = Decompressor.Compress(buf.ToArray());
            var res = new OutBuf();
            res.Add(buf.List.Count);
            res.Add(compr.Length);
            res.Add(compr);
            await SendPacket(type, res, true);
        }

        internal async Task<InBuf> RecievePacket()
        {
            var bytes = await Receive(9);
            var hdr = UDT.ReadStruct<PacketHdr>(bytes, 0, 9);
            bytes = await Receive(hdr.PacketSize);
            return new InBuf(await Decryptor.Decrypt(bytes), hdr);
        }

        static readonly object IdLock = new object();
        static int Id = 0;

        int GetSendId()
        {
            lock (IdLock)
            {
                return Id++;
            }
        }

        async Task SendLogin(Logger log, ConnectLogs process)
        {
			QC.OnConnectCall(null, ConnectProgress.SendLogin);
            if (ExLogin == null)
            {
				log.trace("Send login");
				process.Progress += ", send login";
                byte[] buf = new byte[34];
                buf[0] = (byte)DateTime.Now.Ticks; // 0x7d; 
                buf[1] = 0; //  m_ConnectInfo.m_Server.m_nSrvType
                if (QC.Build != 0)
                    BitConverter.GetBytes((short)QC.Build).CopyTo(buf, 2); //MT5 Terminal build
                else
                    BitConverter.GetBytes((short)CLIENT_BUILD).CopyTo(buf, 2); //MT5 Terminal build
                BitConverter.GetBytes((short)20813).CopyTo(buf, 4); //MQ
                BitConverter.GetBytes(QC.User).CopyTo(buf, 6);
                if (QC.HardwareId == null)
                    QC.HardwareId = CreateHardId(QC.User);
                var key = QC.HardwareId;
                key.CopyTo(buf, 14);
                BitConverter.GetBytes(new Random().Next()).CopyTo(buf, 30); //TODO random 0x11528a15
                byte[] pack = new byte[9 + buf.Length];
                pack[0] = 0;
                BitConverter.GetBytes(buf.Length).CopyTo(pack, 1);
                BitConverter.GetBytes((ushort)GetSendId()).CopyTo(pack, 5); //ID
                BitConverter.GetBytes((ushort)2).CopyTo(pack, 7); //Flags PHF_COMPLETE
                Crypt.EasyCrypt(buf).CopyTo(pack, 9);
                await Send(pack);
            }
            else
            {
				log.trace("Send ex login");
				process.Progress += ", send ex login";
				ExLoginWebsocket = ExLogin.Init(Sock).Result;
				ExLogin.TransferPacketFromTerminalToMtServer();
			}
            byte[] res = await Receive(9);
            var hdr = UDT.ReadStruct<PacketHdr>(res, 0, 9);
            if (hdr.Type != 0 || hdr.PacketSize != 32)
                throw new Exception("SendAccountPassword expected");
            res = Crypt.EasyDecrypt(await Receive(hdr.PacketSize));
            var rec = UDT.ReadStruct<BuildRec>(res, 0, 32);
            if (rec.StatusCode != Msg.DONE)
                throw new ServerException(rec.StatusCode);
            SignData = rec.SignData;
            await SendAccountPassword(log, process);
        }

		static byte[] CreateHardId(ulong user)
		{
			uint seed = (uint)user;
			byte[] data = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				seed = seed * 214013 + 2531011;
				data[i] = (byte)((seed >> 16) & 0xFF);
			}
			MD5 md = new MD5CryptoServiceProvider();
			var hardId = md.ComputeHash(data);
			hardId[0] = 0;
			for (int i = 1; i < 16; i++)
				hardId[0] += hardId[i];
			return hardId;
		}

		public void Disconnect()
        {
            //SendEncode(new byte[] { 0xD });
            //Thread.Sleep(100);
            Close();
        }

        internal void Decompress(InBuf buf)
        {
            int realSize = buf.Int();
            int comprSize = buf.Int();
            byte[] data = buf.Bytes(comprSize);
            byte[] res = Decompressor.Decompress(data, realSize);
            buf.SetBuf(res);
        }

        byte[] SignData;
        byte[] RandData;

		async Task SendAccountPassword(Logger log, ConnectLogs process)
		{
			process.Progress += ", send password";
			log.trace("SendPassword");
			if (QC.Password.Length > 16)
				QC.Password = QC.Password.Substring(0, 16);
			QC.OnConnectCall(null, ConnectProgress.SendAccountPassword);
			byte[] buf = new byte[8 + QC.Password.Length * 2 + 2 * 2];
			BitConverter.GetBytes(QC.User).CopyTo(buf, 0);
			Encoding.Unicode.GetBytes(QC.Password).CopyTo(buf, 8);
			Encoding.Unicode.GetBytes("MQ").CopyTo(buf, 8 + QC.Password.Length * 2);
			MD5Managed md = new MD5Managed();
			md.HashCore(buf, 0, buf.Length);
			md.HashFinal();
			byte[] key = md.Hash.Clone() as byte[];
			md.Initialize(false);
			md.HashCore(SignData, 0, SignData.Length);
			md.HashFinal();
			var cryptKey = md.Hash;
			//vLoginSnd1 login = new vLoginSnd1();
			//Array.Copy(CryptKey, 0, login.m_CryptKey, 0, 16);
			var rand = new Random();
			RandData = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				var v = (byte)rand.Next(0, 255);
				RandData[i] = v;
				//RandData[0] += v;
			}

			buf = new byte[0x22];
			BitConverter.GetBytes((ushort)rand.Next()).CopyTo(buf, 0); //s0 TODO rand 0xbbda
			cryptKey.CopyTo(buf, 2); // cryp key
			RandData.CopyTo(buf, 0x12); // rand data 

			if (QC.OtpPassword != null)
			{
				md = new MD5Managed();
				md.HashCore(key, 0, key.Length);
				md.HashCore(SignData, 0, SignData.Length);
				byte[] crt = { 0xDE, 0xE4, 0x6F, 0xB3, 0x17, 0xD1, 0xA2, 0xC2, 0x11, 0x03, 0x45, 0x94, 0xF8, 0x7B, 0xA0, 0xB1 };
				md.HashCore(crt, 0, crt.Length);
				md.HashCore(new byte[1], 0, 1); //m_ConnectInfo.m_Server.m_nSrvType
				md.HashFinal();
				byte[] oth = md.Hash;
				byte[] otpBytes = Encoding.Unicode.GetBytes(QC.OtpPassword);
				Array.Resize(ref otpBytes, otpBytes.Length + 2);
				otpBytes = Crypt.Encode(otpBytes, oth);
				OutBuf outbuf = new OutBuf();
				outbuf.ByteToBuffer(0x12);
				outbuf.LongToBuffer((uint)otpBytes.Length);
				outbuf.DataToBuffer(otpBytes);

				//outbuf.ByteToBuffer(0x1D);
				//outbuf.LongToBuffer(4);
				//outbuf.LongToBuffer(0);
				int start = buf.Length;
				Array.Resize(ref buf, buf.Length + outbuf.List.Count);
				Array.Copy(outbuf.List.ToArray(), 0, buf, start, outbuf.List.Count);

			}
			byte[] pack = new byte[9 + buf.Length];
			pack[0] = 1; //Type
			BitConverter.GetBytes(buf.Length).CopyTo(pack, 1); //size
			BitConverter.GetBytes((ushort)GetSendId()).CopyTo(pack, 5); //ID
			BitConverter.GetBytes((ushort)2).CopyTo(pack, 7); //Flags PHF_COMPLETE
			Crypt.EasyCrypt(buf).CopyTo(pack, 9);
			await Send(pack);
			await AcceptAuthorized(log, process);
		}

        public string ServerName;
        public byte[] SrvCert;
        public byte[] AesKey;
        public byte[] ProtectKey;
        //public byte[] CryptKey;
       

        public short TradeBuild;
        public short SymBuild;
        public ulong LoginId;
		public ulong LoginIdEx;
        internal ulong CLIENT_BUILD = 5400;

        async Task AcceptAuthorized(Logger log, ConnectLogs process)
        {
            process.Progress += ", accept authorized";
            log.trace("AcceptAuthorized");
            QC.OnConnectCall(null, ConnectProgress.AcceptAuthorized);
            byte[] res = await Receive(9);
            var hdr = UDT.ReadStruct<mt5.PacketHdr>(res, 0, 9);
            if (hdr.Type != 1) //|| hdr.PacketSize != 32
                throw new Exception("AcceptAuthorized expected");
            res = Crypt.EasyDecrypt(await Receive(hdr.PacketSize));
            var rec = UDT.ReadStruct<LoginRcv1>(res, 0, 0x2C);
            if (rec.StatusCode != Msg.DONE && rec.StatusCode != Msg.ADVANCED_AUTHORIZATION)
                throw new ServerException(rec.StatusCode);
            TradeBuild = rec.TradeBuild;
            SymBuild = rec.SymBuild;
            InBuf buf = new InBuf(res, 0x2C);
            while (buf.hasData)
            {
                switch (buf.Byte())
                {
                    case 0:
                        ServerName = buf.Str();
                        break;
                    case 1:
                        SrvCert = buf.ByteAr();
                        break;
                    case 7:
                        AesKey = buf.ByteAr();
                        var key = GetCryptKey(AesKey);
                        Encryptor = new PackEncrypt(key);
                        Decryptor = new PackDecrypt(key);
                        break;
                    case 0xA:
                        buf.ByteAr();//bStatus = bufMan.GetRecord(pConnectTime, szConnectTime);
                        break;
                    case 0xB:
                        buf.ByteAr();//bStatus = bufMan.GetRecord(pNetAddr, szNetAddr);
                        break;
                    case 0xC:
                        buf.ByteAr();//bStatus = bufMan.GetRecord(varÑ4, varC0);
                        break;
                    case 0x13:
                        buf.ByteAr(); //bStatus = bufMan.GetRecord(pOneTimeKey, szOneTimeKey);
                        break;
                    case 0x1B:
                        ProtectKey = GetProtectKey(buf.ByteAr());
                        break;
                    case (byte)0x1C:
                        LoginId = GetLoginId(buf.ByteAr(), process);
                        break;
					case 0x23:
						LoginIdEx = GetLoginIdEx(buf.ByteAr(), process);
						break;
					default:
                        buf.ByteAr();
                        break;
                }
            }
            if (rec.StatusCode == Msg.ADVANCED_AUTHORIZATION)
            {
                X509Certificate2Collection collection = new X509Certificate2Collection();
                collection.Import(QC.PfxFile, QC.PfxFilePassword, X509KeyStorageFlags.Exportable);
                byte[] sign = null;
                byte[] encodedCert = null;
                X509Certificate2 certif = null;
                foreach (X509Certificate2 item in collection)
                {
                    if (item.GetRSAPrivateKey() != null)
                    {
                        encodedCert = item.GetRawCertData();
                        certif = item;
                    }
                }
                if (certif == null)
                    throw new ConnectionException("RSA private key not found");
                using (RSA rsa = certif.GetRSAPrivateKey())
                    if (rsa != null)
                        sign = rsa.SignData(SignData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                Array.Reverse(sign, 0, sign.Length);
                var ob = new OutBuf();
                ob.DataToBuffer(new byte[16]);
                ob.ByteToBuffer(4);
                ob.LongToBuffer((uint)sign.Length);
                ob.DataToBuffer(sign);
                ob.ByteToBuffer(3);
                ob.LongToBuffer((uint)encodedCert.Length);
                ob.DataToBuffer(encodedCert);
                await SendPacket(2, ob, false);
            }
            await RequestTradeInfo(process);
        }

        ulong GetLoginId(byte[] loginhash, ConnectLogs process)
        {
            process.Progress += ", get login id";
		    ulong loginId;
#if TRIAL
            if (TradeBuild >= 4852)
                loginId = new LoginIdWebServer().Decode(TrialLoginIdPath + "/CheckMT5", QC.ApiKey, loginhash, QC.LoginIdWebServerTimeout, true);
            else
                loginId = new LoginIdWebServer().Decode(TrialLoginIdPath + "/CheckMT5", QC.ApiKey, loginhash, QC.LoginIdWebServerTimeout, false);
#else
            if (TradeBuild >= 4852)
                loginId = new LoginIdWebServer().Decode(QC.LoginIdPath + "/DecodeNew", QC.ApiKey, loginhash, QC.LoginIdWebServerTimeout);
            else
                loginId = new LoginId().Decode(loginhash);
#endif
            process.Progress += ", got login id";
            return loginId ^ QC.User ^ CLIENT_BUILD ^ BitConverter.ToUInt64(SignData, 0) ^ 0x05286AED3286692A;
        }

		ulong GetLoginIdEx(byte[] loginhash, ConnectLogs process) 
        {
			process.Progress += ", get login id ex";
			ulong loginId;
            loginId = new LoginIdWebServer().Decode(QC.LoginIdPath + "/DecodeEx", QC.ApiKey, loginhash, QC.LoginIdWebServerTimeout);
			process.Progress += ", got login id ex";
            var res = loginId ^ QC.User ^ CLIENT_BUILD ^ BitConverter.ToUInt64(SignData, 0);
            if (TradeBuild >= 4852)
                res ^= 0x0004367468243443;
            res ^= 0x05286AED3286692A;
            return res;
		}

        byte[] GetProtectKey(byte[] protectKey)
        {
            //protectKey = new byte[] { 0xb2, 0x7c, 0xbb, 0xc5, 0x83, 0x19, 0xfd, 0xee, 0x9e, 0xa4, 0x5d, 0xd6, 0xfb, 0x6f, 0xc8, 0x59, 0xcc, 0x8a, 0xfd, 0x2c, 0x04, 0xb8, 0xda, 0x84, 0x8c, 0x0b, 0xbb, 0x87, 0xb5, 0x3f, 0xca, 0x7d, 0x09, 0x20, 0x04, 0x5e, 0x82, 0x87, 0xef, 0xf0, 0xeb, 0x90, 0x4b, 0x72, 0x0a, 0x47, 0x4b, 0xd6, 0xe9, 0xe4, 0x80, 0x25, 0x95, 0x7a, 0xe1, 0xc5, 0x59, 0x5c, 0xa7, 0xe9, 0x44, 0x26, 0x62, 0x0f, 0x09, 0x3d, 0xf0, 0x0f, 0xe7, 0x95, 0x76, 0x86, 0x69, 0x49, 0x58, 0xaf, 0x92, 0x62, 0xb7, 0x77, 0x14, 0x7e, 0x1c, 0x67, 0xfd, 0x34, 0x38, 0x6d, 0x1a, 0x94, 0xc2, 0x0e, 0x5b, 0xf2, 0xbe, 0x9d, 0xf7, 0x8a, 0x99, 0xf3, 0x07, 0x6c, 0x15, 0x4e, 0x70, 0xe1, 0x29, 0x12, 0x90, 0x2a, 0xe1, 0x65, 0x5a, 0xb5, 0x81, 0x5e, 0x8e, 0xdf, 0xac, 0xef, 0xba, 0xf6, 0x80, 0xe9, 0x9b, 0x96, 0x40, 0x7e };
            var buf = new byte[8 + QC.Password.Length * 2 + 2 * 2];
            BitConverter.GetBytes(QC.User).CopyTo(buf, 0);
            Encoding.Unicode.GetBytes(QC.Password).CopyTo(buf, 8);
            Encoding.Unicode.GetBytes("MQ").CopyTo(buf, 8 + QC.Password.Length * 2);
            MD5Managed md = new MD5Managed();
            md.HashCore(buf, 0, buf.Length);
            md.HashFinal();
            byte[] key = md.Hash;
            byte[] data = new byte[protectKey.Length + 16];
            key.CopyTo(data, protectKey.Length);
            Crypt.Decode(protectKey, key).CopyTo(data, 0);
            SHA256 sha = SHA256.Create();
            sha.ComputeHash(data);
            return sha.Hash;
        }

        public async Task RequestTradeInfo(ConnectLogs process)
        {
            process.Progress += ", request trade info";
            QC.OnConnectCall(null, ConnectProgress.RequestTradeInfo);
            //OutBuf ping = new OutBuf();
            //ping.CreateHeader(10, GetSendId());
            //Send(ping.Bytes);

            OutBuf buf = new OutBuf();
            buf.ByteToBuffer(0x22);                                //cmd
            buf.LongToBuffer(8);                                   //size
            buf.LongLongToBuffer(-1);                              //data
            buf.ByteToBuffer(0x27);                                //cmd
            buf.LongToBuffer(4);                                   //size
            buf.LongToBuffer(0);                       //data m_Config.s10E TODO?
                                                       // NEW m_sUtmCampaign and etc...
                                                       //	if (m_Cfg.m_Common.m_bNewsEnable)
                                                       //	{
                                                       //		pBufMan->ByteToBuffer(0x19);							//cmd (config News)
                                                       //		pBufMan->LongToBuffer((m_Cfg.m_Common.m_nNumberLanguages + 1) * 4);		//size
                                                       //		pBufMan->LongToBuffer(m_Cfg.m_Common.m_nNumberLanguages);				//data
                                                       //		pBufMan->DataToBuffer(m_Cfg.m_Common.m_nNewsLanguages, m_Cfg.m_Common.m_nNumberLanguages * 4);	//data
                                                       //		pBufMan->ByteToBuffer(0x16);							//cmd (request News)
                                                       //		pBufMan->LongToBuffer(9);								//size
                                                       //		pBufMan->ByteToBuffer(1);								//data
                                                       //		pBufMan->LongLongToBuffer(m_News.GetFileTime());		//data
                                                       //	}
                                                       //	else
                                                       //	{
                                                       //		pBufMan->ByteToBuffer(0x16);							//cmd (request News)
                                                       //		pBufMan->LongToBuffer(9);								//size
                                                       //		pBufMan->ByteToBuffer(0);								//data
                                                       //		pBufMan->LongLongToBuffer(0);							//data
                                                       //	}
            var softid = CreateSoftId();
            buf.ByteToBuffer(0x6C);                            //cmd (send soft id)
            buf.LongToBuffer(8);                               //size
            buf.LongLongToBuffer(softid);                          //data

            buf.ByteToBuffer(0x18);                                //cmd (request Mails)
            buf.LongToBuffer(8);                                   //size
            buf.LongLongToBuffer(0);       //data m_Mails.GetLastMailTime()

            if (CLIENT_BUILD > 4200)
            {
                buf.ByteToBuffer(0x84);                                //cmd (send application info)
                buf.LongToBuffer(0);                                   //size
                buf.ByteToBuffer(0x7F);
                var bytes = CreateHardId(QC.User); // 16 bytes

                // Convert bytes to hex string
                string hex = BitConverter.ToString(bytes).Replace("-", "");

                // Use different slices of the hex string for each section
                string osVerPart = hex.Substring(0, 3);             // first 3 chars
                string osId1 = hex.Substring(3, 4);
                string osId2 = hex.Substring(7, 4);
                string osId3 = hex.Substring(11, 4);

                var info =
                    $"file=terminal64.exe" +
                    $"\tversion={CLIENT_BUILD}" +
                    $"\tcert_company=MetaQuotes Ltd" +
                    $"\tcert_issuer=DigiCert Trusted G4 Code Signing RSA4096 SHA384 2021 CA1" +
                    $"\tcert_serial=04390a4c5f8906a1d7052c1768d45047" +
                    $"\tos_ver=Windows 11 build 22{osVerPart}" +
                    $"\tos_id={osId1}-{osId2}-{osId3}-AAOEM" +
                    $"\tcomputer={GenerateComputerName(bytes)}\t";
                //var info = "file=terminal64.exe\tversion=4353\tcert_company=MetaQuotes Ltd\tcert_issuer=DigiCert Trusted G4 Code Signing RSA4096 SHA384 2021 CA1\tcert_serial=04390a4c5f8906a1d7052c1768d45047\tos_ver=Windows 11 build 22631\tos_id=00342-43252-11000-AAOEM\tcomputer=OMEN\t";
                var infoBytes = Encoding.Unicode.GetBytes(info);
                Array.Resize(ref infoBytes, infoBytes.Length + 2);
                buf.LongToBuffer((uint)infoBytes.Length);
                buf.DataToBuffer(infoBytes);
            }
			process.Progress += ", request symbols";
            RequestSymbols(buf);                          //request Symbols
            RequestSpreads(buf);                          //request Spreads
            RequestTickers(buf);                          //request Tickers
            AcceptLoginId(buf);

			await SendPacket(0xC, buf);
            process.Progress += ", request trade info done";
        }

        string GenerateComputerName(byte[] bytes)
        {
            // Use a simple name seed from bytes
            const string consonants = "BCDFGHJKLMNPQRSTVWXYZ";
            const string vowels = "AEIOU";

            // Derive a numeric seed from the bytes
            int seed = BitConverter.ToInt32(bytes, 0);
            Random rng = new Random(seed);

            // Generate a short pseudo-human name (e.g. "LUMO", "KERI", "DAVO")
            StringBuilder name = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                name.Append(consonants[rng.Next(consonants.Length)]);
                name.Append(vowels[rng.Next(vowels.Length)]);
            }

            // Add optional suffix based on next byte for variety
            string[] suffixes = { "-PC", "-DESKTOP", "-LAPTOP", "-HOME", "-OFFICE" };
            string suffix = suffixes[bytes.Last() % suffixes.Length];

            return name.ToString() + suffix;
        }

        string RandomString(int length)
		{
			Random random = new Random();
			string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		string RandomNumericString(int length)
		{
			Random random = new Random();
			string chars = "0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		long CreateSoftId()
		{
            MD5 md = new MD5CryptoServiceProvider();
            var id = md.ComputeHash(BitConverter.GetBytes(QC.User));
            return BitConverter.ToInt64(id, 0);
		}

		/*void vSubscribeClient::RequestTicks()
        {
            if (m_pClient->GetServerStatus() != vAcceptAccount)
                return;
            vSockBufManager bufMan(0);
            m_Crit.EnterCriticalSection();
            bufMan.ByteToBuffer(9);
            bufMan.LongToBuffer(m_arrTicks.GetSize());
            for (int i = 0; i < m_arrTicks.GetSize(); i++)
                bufMan.LongToBuffer(m_arrTicks[i]->m_SubSym.m_nId);
            m_bRequest = true;
            m_Crit.LeaveCriticalSection();
            m_pClient->await SendPacket(0x69, &bufMan);
        }*/



		byte[] GetCryptKey(byte[] aesKey)
        {
            byte[] buf = new byte[8 + QC.Password.Length * 2 + 2 * 2];
            BitConverter.GetBytes(QC.User).CopyTo(buf, 0);
            Encoding.Unicode.GetBytes(QC.Password).CopyTo(buf, 8);
            Encoding.Unicode.GetBytes("MQ").CopyTo(buf, 8 + QC.Password.Length * 2);
            MD5Managed md = new MD5Managed();
            md.HashCore(buf, 0, buf.Length);
            md.HashFinal();
            byte[] key = md.Hash;
            //md.Initialize(false);
            //md.HashCore(RandData, 0, RandData.Length);
            //md.HashFinal();
            //var cryptKey = md.Hash;
            //bStatus = memcmp(cryptKey, login.m_CryptKey, 16) == 0;
            //if (!bStatus) return INVALID_SERVER;
            //aesKey = new byte[]{0x41, 0x23, 0xde, 0xfb, 0x6b, 0xb3, 0x40, 0xdb, 0xfc, 0xb0, 0x7a, 0xd9, 0x34, 0xa8, 0xf0, 0x9f, 0xaf, 0x1b, 0x31, 0x4d, 0x93, 0xe0, 0xcc, 0x05, 0x0e, 0xc3, 0x65, 0xad, 0x00, 0x73, 0x07, 0xf0, 0xf4, 0x9c, 0xbc, 0x86, 0x55, 0x50, 0x5b, 0xb6, 0xdf, 0xab, 0xa2, 0x19, 0x53, 0xb7, 0x84, 0xa0, 0x6f, 0x8d, 0xc4, 0xd8, 0x1d, 0xbd, 0xc3, 0x6d, 0xeb, 0xf0, 0x97, 0xee, 0xbb, 0xcc, 0x5c, 0xcf, 0xbb, 0x18, 0xcf, 0xb4, 0x98, 0x21, 0x19, 0xed, 0xef, 0x5c, 0xe8, 0x3c, 0x05, 0x4c, 0xc4, 0xdf, 0xb7, 0xa6, 0xa1, 0xca, 0xb4, 0xb4, 0xb0, 0x35, 0xe9, 0xf7, 0x7b, 0x56, 0x3d, 0x10, 0x31, 0x70, 0x7f, 0xe1, 0x3e, 0x0c, 0x9c, 0xef, 0x1f, 0x86, 0x16, 0x0a, 0x75, 0xcc, 0xb1, 0x31, 0x58, 0x63, 0x70, 0xb0, 0xed, 0xab, 0xbf, 0x8b, 0x3b, 0x63, 0xf2, 0x1f, 0x3a, 0x6f, 0xee, 0x07, 0x2d, 0xd9, 0x26, 0x3d, 0x32, 0x17, 0xca, 0x81, 0x18, 0x8c, 0x3a, 0xff, 0x70, 0x51, 0xc1, 0x2c, 0xe7, 0x34, 0x80, 0xf1, 0xd3, 0x01, 0xa8, 0x0b, 0x0b, 0x01, 0xed, 0xb2, 0xfc, 0xc1, 0x37, 0x78, 0xf9, 0x14, 0x9a, 0x75, 0xd3, 0x5b, 0x88, 0xa1, 0xaa, 0x04, 0x45, 0x81, 0x02, 0x51, 0x9c, 0x06, 0x19, 0x5b, 0xd1, 0xb2, 0x79, 0x56, 0xd6, 0xfc, 0xc9, 0x16, 0xc1, 0xf6, 0xe8, 0xd2, 0x7e, 0x2d, 0x3d, 0x29, 0xc1, 0xd0, 0x48, 0x62, 0x3f, 0x15, 0x7d, 0xf8, 0x1e, 0xd9, 0x53, 0x56, 0xaa, 0x87, 0x98, 0xdf, 0x49, 0x3a, 0x07, 0x31, 0xb1, 0x26, 0x1c, 0xab, 0x58, 0x34, 0x27, 0x2d, 0x2c, 0xec, 0xa0, 0x1e, 0x85, 0x98, 0xba, 0xb4, 0x58, 0xa6, 0x3a, 0x6f, 0xad, 0x4f, 0x8a, 0xe7, 0x53, 0x76, 0x09, 0xc8, 0xd2, 0xd3, 0xa6, 0x1b, 0xa1, 0x50, 0xad, 0xc7, 0x99, 0xb3, 0xe0, 0x57, 0xaa, 0x7e, 0xca, 0xfd};
            vAES aes = new vAES();
            return aes.EncryptData(aesKey, key);
        }

        void AcceptLoginId(OutBuf buf)
        {
            buf.ByteToBuffer(0x58);                                //cmd
            buf.LongToBuffer(8);                                   //size
            buf.Add(LoginId ^ 0x5286AED3286692A);   //data
            if (CLIENT_BUILD > 4200)
            {
                buf.ByteToBuffer(0x86);                                    //cmd
                buf.LongToBuffer(8);                                       //size
                buf.Add(LoginIdEx ^ 0x05286AED3286692A);   //data
            }
		}

        void RequestSymbols(OutBuf buf)
        {
            //MD5Managed md = new MD5Managed();
            //md.HashCore(new byte[16], 0 , 16);
            //md.HashFinal();
            var key = new byte[] { 0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04, 0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e };
            buf.ByteToBuffer(7);
            buf.LongToBuffer(0x2C);
            buf.LongLongToBuffer(0); //time
            buf.LongToBuffer(0);//m_arrSym.GetSize()
            buf.DataToBuffer(key);
            buf.DataToBuffer(key);
        }

        void RequestSpreads(OutBuf buf)
        {
            //MD5Managed md = new MD5Managed();
            //md.HashCore(new byte[16], 0 , 16);
            //md.HashFinal();
            var key = new byte[] { 0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04, 0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e };
            buf.ByteToBuffer(0x28);
            buf.LongToBuffer(28);
            buf.LongLongToBuffer(0); //time
            buf.LongToBuffer(0);//m_arrSym.GetSize()
            buf.DataToBuffer(key);
        }

        void RequestTickers(OutBuf buf)
        {
            buf.ByteToBuffer(0x11);
            buf.LongToBuffer(16);
            buf.DataToBuffer(new byte[16]);
        }

        //byte[] GetBytes<T>(T str, int size)
        //{
        //    //int size = Marshal.SizeOf(str);
        //    byte[] arr = new byte[size];

        //    IntPtr ptr = Marshal.AllocHGlobal(size);
        //    Marshal.StructureToPtr(str, ptr, true);
        //    Marshal.Copy(ptr, arr, 0, size);
        //    Marshal.FreeHGlobal(ptr);
        //    return arr;
        //}

        public async Task Connect(CancellationToken cancellation)
        {
            try
            {
                if (QC.ProxyEnable)
                    await Connect(QC.Host, QC.Port, QC.ProxyHost, QC.ProxyPort, QC.ProxyUser, QC.ProxyPassword, QC.ProxyType, cancellation);
                else
                    await Connect(QC.Host, QC.Port, cancellation);
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }
    }
}
