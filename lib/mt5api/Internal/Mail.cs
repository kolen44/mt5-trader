using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mtapi.mt5
{

	internal class Mail
	{
		internal readonly MT5API QC;

		internal List<MailMessage> Messages = new List<MailMessage>();

		public Mail(MT5API qc)
		{
			QC = qc;
		}


		internal void Parse(InBuf buf)
		{
			var contTime = buf.Long();
			int size = buf.Int();
			ushort type = buf.UShort();
			var msg = new MailMessage();
			while (buf.Left > 0)
			{
				size = buf.Int();
				type = buf.UShort();
				switch(type)
				{
					case 1:     // Main record
						msg.Id = buf.Long();
						//SendMailRequest(msg.Id);
						var group = buf.Long();
						msg.Time = ConvertTo.DateTime(buf.Long());
						var fromMain = buf.ULong();
						var toMain = buf.ULong();
						var flags = buf.Byte();
						break;
					//case 2:
					//	s4B = (BYTE*)(pHdr + 1);
					//	s53 = pHdr->m_nSize;
					//	break;
					case 3:     // Subject
						msg.Subject = ConvertTo.String(buf.Bytes(size));
						break;
					case 4:     // From
						msg.From = ConvertTo.String(buf.Bytes(size));
						break;
					case 5:     // To
						msg.To = ConvertTo.String(buf.Bytes(size));
						break;
					//case 6:
					//	s93 = (BYTE*)(pHdr + 1);
					//	s9B = pHdr->m_nSize / 16;
					//	break;
					//case 8:     // Attach
					//	m_pAttach = (BYTE*)(pHdr + 1);
					//	m_nAttachSize = pHdr->m_nSize;
					//	break;
					//case 9:     // Certificate serial number
					//	if (pHdr->m_nSize == 8)
					//		m_nCertSn = *(LONGLONG*)(pHdr + 1);
					//	break;
					//case 10:    // Compressed letter context
					//	m_pContext = (BYTE*)(pHdr + 1);
					//	m_nContextSize = pHdr->m_nSize;
					//	break;
					//case 11:
					//	s77 = (BYTE*)(pHdr + 1);
					//	s7F = pHdr->m_nSize;
					//	break;
					//case 12:
					//	s63 = (BYTE*)(pHdr + 1);
					//	s6B = pHdr->m_nSize;
					//	break;
					//case 13:
					//	s9F = (BYTE*)(pHdr + 1);
					//	sA7 = pHdr->m_nSize / 8;
					//	break;
					//default:
					//	throw new NotFiniteNumberException("Mail type " + type);
				} 
			}
			Messages.Add(msg);
			QC.OnMailCall(msg);
		}

		internal void ParseBody(InBuf buf)
		{
			var status = buf.Int();
			if (status != 0)
				throw new ServerException((Msg)status);
			var bData = buf.Byte();
			var num = buf.Int();
			var msg = new MailMessage();
			for (int i = 0; i < num; i++)
			{
				var size = buf.Int();
				var type = buf.UShort();
				switch (type)
				{
					case 1:     // Main record
						msg.Id = buf.Long();
						//SendMailRequest(msg.Id);
						var group = buf.Long();
						msg.Time = ConvertTo.DateTime(buf.Long());
						var fromMain = buf.ULong();
						var toMain = buf.ULong();
						var flags = buf.Byte();
						break;
					//case 2:
					//	s4B = (BYTE*)(pHdr + 1);
					//	s53 = pHdr->m_nSize;
					//	break;
					case 3:     // Subject
						msg.Subject = ConvertTo.String(buf.Bytes(size));
						break;
					case 4:     // From
						msg.From = ConvertTo.String(buf.Bytes(size));
						break;
					case 5:     // To
						msg.To = ConvertTo.String(buf.Bytes(size));
						break;
					case 24:     // body
						var b = buf.Bytes(size);

						//var from = new MemoryStream(b);
						//var to = new MemoryStream();
						//var gZipStream = new GZipStream(from, CompressionMode.Decompress);
						//gZipStream.CopyTo(to);
						//var r = to.ToArray();
						//try
						//{
						//	QC.Connection.Decompress(new InBuf(b, 0));
						//}
						//catch (Exception)
						//{
						//}
						//try
						//{
						//	using (var compressedStream = new MemoryStream(b))
						//	using (var zipStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress))
						//	using (var resultStream = new MemoryStream())
						//	{
						//		zipStream.CopyTo(resultStream);
						//		var data = resultStream.ToArray();
						//	}
						//}
						//catch (Exception)
						//{

						//}
						msg.Body = System.Text.Encoding.Unicode.GetString(b);
						break;
					//case 6:
					//	s93 = (BYTE*)(pHdr + 1);
					//	s9B = pHdr->m_nSize / 16;
					//	break;
					//case 8:     // Attach
					//	m_pAttach = (BYTE*)(pHdr + 1);
					//	m_nAttachSize = pHdr->m_nSize;
					//	break;
					//case 9:     // Certificate serial number
					//	if (pHdr->m_nSize == 8)
					//		m_nCertSn = *(LONGLONG*)(pHdr + 1);
					//	break;
					//case 10:    // Compressed letter context
					//	m_pContext = (BYTE*)(pHdr + 1);
					//	m_nContextSize = pHdr->m_nSize;
					//	break;
					//case 11:
					//	s77 = (BYTE*)(pHdr + 1);
					//	s7F = pHdr->m_nSize;
					//	break;
					//case 12:
					//	s63 = (BYTE*)(pHdr + 1);
					//	s6B = pHdr->m_nSize;
					//	break;
					//case 13:
					//	s9F = (BYTE*)(pHdr + 1);
					//	sA7 = pHdr->m_nSize / 8;
					//	break;
					//default:
					//	throw new NotFiniteNumberException("Mail type " + type);
				}
			}
			Messages.Add(msg);
			QC.OnMailCall(msg);
		}


		public void SendMailRequest(long id)
		{
			OutBuf buf = new OutBuf();
			buf.ByteToBuffer(8);
			buf.LongLongToBuffer(id);
			QC.Connection.SendPacket(0x68, buf).Wait();
		}
	}

	public class MailMessage
	{
		public long Id;
		public DateTime Time;
		public string From;
		public string To;
		public string Subject;
		public string Body;
	}		

}
