using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
	internal class Http
	{
		class DownloadStringResult
		{
			public string Res = null;
			public Exception Ex = null;
		}

		string Url;

		public string DownloadString(string url, int timeout)
		{
			Url = url;
			Thread thread = new Thread(ThreadStart);
			var res = new DownloadStringResult();
			thread.Start(res);
			if (!thread.Join(timeout))
				throw new TimeoutException($"DownloadString timeout({url}) in " + timeout + "ms");
			if (res.Ex != null)
				throw res.Ex;
			return res.Res;
		}

		void ThreadStart(object param)
		{
			var result = (DownloadStringResult)param;
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(Url);
				request.Method = "GET";
				request.ContentType = "application/text";
				var resp = (HttpWebResponse)request.GetResponse();
				result.Res = new StreamReader(resp.GetResponseStream()).ReadToEnd();
			}
			catch (Exception ex)
			{
				result.Ex = new Exception($"DownloadString({Url}): " + ex.Message);
				return;
			}
		}
	}
}
