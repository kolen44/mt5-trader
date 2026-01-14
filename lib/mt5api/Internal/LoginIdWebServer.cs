#if LoginDLL
#else
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
    internal class LoginIdWebServer
    {
        class Result
        {
            public ulong Id = 0;
            public Exception Ex = null;
        }

        public ulong Decode(string url, string guid, byte[] bytes, int timeout, bool decodeNew = false)
        {
            for (int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(delegate (object param)
                {
                    var result = (Result)param;
                    string response = null;
                    ConnectException resultException = null;
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(url + "?guid=" + guid);
                        var str = Convert.ToBase64String(bytes);
                        if (decodeNew)
                            str = "loginidnew5" + str;
                        var data = Encoding.ASCII.GetBytes(str);
                        request.Method = "POST";
                        request.ContentType = "application/text";
                        request.ContentLength = data.Length;
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        var resp = (HttpWebResponse)request.GetResponse();
                        response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                        if (resp.StatusCode == HttpStatusCode.Created)
                            throw new Exception(response);
                        if (resp.StatusCode == HttpStatusCode.OK)
                            resultException = null;
                    }
                    catch (Exception ex)
                    {
                        resultException = new ConnectException($"LoginIdWebServer({url}): " + ex.Message);
                    }
                    if (resultException != null)
                    {
                        result.Ex = resultException;
                        return;
                    }
                    ulong id;
                    if (ulong.TryParse(response, out id))
                        result.Id = id;
                    else
                        result.Ex = new ConnectException($"LoginIdWebServer response({url}): " + response);
                });

                var res = new Result();
                thread.Start(res);
                if (!thread.Join(timeout / 2))
                    if (i == 2)
                        throw new ConnectException($"No reply from login id web server({url}) in " + timeout + "ms");
                    else
                        continue;
                if (res.Ex != null)
                    if (i == 2)
                        throw res.Ex;
                    else
                        continue;
                return res.Id;
            }
            throw new ConnectException($"Cannot get login id from web server({url})");
        }
    }
}
#endif