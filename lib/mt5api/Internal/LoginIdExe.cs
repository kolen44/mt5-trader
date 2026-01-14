using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtapi.mt5
{
	class LoginIdExe
	{
		public static ulong Get(byte[] data, string path)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.FileName = path;
			startInfo.WorkingDirectory = new FileInfo(path).DirectoryName;
			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			using (Stream stdout = process.StandardInput.BaseStream)
			{
				stdout.Write(BitConverter.GetBytes(data.Length), 0, 4);
				stdout.Write(data, 0, data.Length);
				stdout.Flush();
			}
			using (Stream stdin = process.StandardOutput.BaseStream)
			{
				byte[] buf = new byte[4];
				int count = stdin.Read(buf, 0, buf.Length);
				if (count < 4)
					throw new Exception("Cannot read header");
				int len = BitConverter.ToInt32(buf, 0);
				buf = new byte[len];
				count = stdin.Read(buf, 0, buf.Length);
				if (count != buf.Length)
					throw new Exception("Wrong input");
				if (count != 8)
					throw new Exception(Encoding.UTF8.GetString(buf));
				return BitConverter.ToUInt64(buf, 0);
			}
		}
	}
}
