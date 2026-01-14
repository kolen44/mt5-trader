using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    public class Broker
    {
        internal static ConcurrentDictionary<string, KeyValuePair<IList<Broker.Company>, DateTime>> SearchHistory
           = new ConcurrentDictionary<string, KeyValuePair<IList<Broker.Company>, DateTime>>(StringComparer.OrdinalIgnoreCase);

        public static IList<Company> Search(string company)
        {
            if (company == null)
                throw new ArgumentNullException("company patameter is null");
            if (company.Length < 2)
                throw new Exception("Specify at least 2 symbols");
            if (Broker.SearchHistory == null)
                Broker.SearchHistory = new ConcurrentDictionary<string, KeyValuePair<IList<Broker.Company>, DateTime>>();
            if (Broker.SearchHistory.TryGetValue(company, out var res))
                if (DateTime.Now.Subtract(res.Value).TotalHours < 4)
                    return res.Key;
            try
            {
                string response;
                var request = (HttpWebRequest)WebRequest.Create($"http://search.mtapi.io/Search?company={company}&mt5=true");
                request.Method = "GET";
                var resp = (HttpWebResponse)request.GetResponse();
                response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                response = response.Substring(response.IndexOf("{"));
                var result = ReadToObject(response).result;
                if (result != null)
                    Broker.SearchHistory[company] = new KeyValuePair<IList<Company>, DateTime>(result, DateTime.Now);
                return result;
            }
            catch (Exception)
            {
                if (Broker.SearchHistory.TryGetValue(company, out var result))
                    return res.Key;
                throw;
            }
            
        }

        public static async Task<IList<Company>> SearchAsync(string company)
        {
            return Search(company);
        }

        public static async Task<IList<Company>> SearchMQAsync(string company)
        {
            return SearchMQ(company);
        }

        public static IList<Company> SearchMQ(string company)
        {
            if (company == null)
                throw new ArgumentNullException("company patameter is null");
            if (company.Length < 4)
                throw new Exception("Specify at least 4 symbols");
            if (Broker.SearchHistory.TryGetValue(company, out var res))
                if (DateTime.Now.Subtract(res.Value).TotalHours < 4)
                    return res.Key;
            try
            {
                string req = "company=" + company + "&code=mt5";
                var sign = GetSignature(Encoding.Default.GetBytes(req));
                req += "&signature=";
                foreach (var item in sign)
                {
                    string b = item.ToString("X").ToLower();
                    if (b.Length == 1)
                        b = "0" + b;
                    req += b;
                }
                req += "&ver=2";

               
                var request = (HttpWebRequest)WebRequest.Create("https://updates.metaquotes.net/public/mt5/network");
                var data = Encoding.UTF8.GetBytes(req);

                request.Method = "POST";
                request.Accept = "*/*";
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("Accept-Language", "en");
                request.UserAgent = $"MetaTrader 5 Terminal/5.{new Connection(new MT5API()).CLIENT_BUILD} (Windows NT 10.0.22621; x64)";
                request.Headers.Add("Cookie", GetCookies());
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                string responseText;
                using (var responseStream = response.GetResponseStream())
                {
                    Stream decompressedStream = responseStream;

                    // Check if response is gzip encoded
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    }

                    using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                responseText = responseText.Substring(responseText.IndexOf("{"));
                var result = ReadToObject(responseText).result;
                if (result != null)
                    Broker.SearchHistory[company] = new KeyValuePair<IList<Company>, DateTime>(result, DateTime.Now);
                return result;
            }
            catch (Exception)
            {
                if (Broker.SearchHistory.TryGetValue(company, out var result))
                    return res.Key;
                throw;
            }
            
        }


        static string GetCookies()
        {
            var timestamp = Stopwatch.GetTimestamp();
            var uptime = ((double)timestamp) / Stopwatch.Frequency;
            var uptimeSpan = TimeSpan.FromSeconds(uptime);
            ulong ticks = (ulong)uptimeSpan.TotalMilliseconds;
            ulong time = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - 16436 * 24 * 3600;
            ulong softid = time | ((ticks & 0x1FFFFFF) << 32) | 0x4200000000000000;
            byte[] key = CreateHardId();
            string commonKey = BitConverter.ToString(key).Replace("-", "");
            commonKey = commonKey.Substring(0, 17);
            var age = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - 24 * 3600;
            return $"_fz_uniq={softid};uniq={softid};age={age};tid={commonKey};";
        }

        static byte[] CreateHardId()
        {
            uint seed = (uint)DateTime.Now.Ticks;//522441350;//
            byte[] data = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                seed = seed * 214013 + 2531011;
                data[i] = (byte)((seed >> 16) & 0xFF);
            }
            MD5 md = new MD5CryptoServiceProvider();
            var _HardId = md.ComputeHash(data);
            _HardId[0] = 0;
            for (int i = 1; i < 16; i++)
                _HardId[0] += _HardId[i];
            return _HardId;
        }

        public static Companies ReadToObject(string json)
        {
            var deserializedUser = new Companies();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(deserializedUser.GetType());
            deserializedUser = ser.ReadObject(ms) as Companies;
            ms.Close();
            return deserializedUser;
        }

        public class Result
        {
            public string name { get; set; }
            public string logo_url { get; set; }
            public string site { get; set; }
            public IList<string> access { get; set; }
        }
        public class Company
        {
            public string company { get; set; }
            public IList<Result> results { get; set; }
        }
        public class Companies
        {
            public IList<Company> result { get; set; }
        }



        static byte[] GetSignature(byte[] data)
        {
            MD5Managed md = new MD5Managed();
            md.HashCore(data, 0, data.Length);
            var key = md.HashFinal();
            md.Initialize(true);
            md.HashCore(key, 0, 16);
            byte[] sign = { 0x3D, 0x7B, 0x15, 0x16, 0xD6, 0xEA, 0xBB, 0x34, 0xD9, 0xD6, 0x63, 0xE3, 0x62, 0x3E, 0x1B, 0xD7,
                            0xFB, 0xDC, 0xAE, 0xF4, 0x57, 0x3B, 0xDF, 0x35, 0x7F, 0xA8, 0xCF, 0x0B, 0xBE, 0xAD, 0x92, 0x7F };
            md.HashCore(sign, 0, 32);
            return md.HashFinal();
        }
    }

    public class BrokerSearchResultsAsArray
    {
        public static Company[] Search(string company)
        {
            var list = Broker.Search(company);
            return ConvertToArray(list);

        }

        public static Company[] SearchMQ(string company)
        {
            var list = Broker.SearchMQ(company);
            return ConvertToArray(list);
        }


        static Company[] ConvertToArray(IList<Broker.Company> list)
        {
            var res = new Company[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var comp = new Company();
                comp.CompanyName = list[i].company;
                comp.Results = new Result[list[i].results.Count];
                for (int j = 0; j < list[i].results.Count; j++)
                {
                    comp.Results[j] = new Result();
                    comp.Results[j].Name = list[i].results[j].name;
                    comp.Results[j].Access = list[i].results[j].access.ToArray();
                }
                res[i] = comp;
            }
            return res;
        }


        public class Result
        {
            public string Name { get; set; }
            public string[] Access { get; set; }
        }
        public class Company
        {
            public string CompanyName { get; set; }
            public Result[] Results { get; set; }
        }
        public class Companies
        {
            public Company[] Result { get; set; }
        }
    }
}
