using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace mtapi.mt5
{

    /// <summary>
    /// All symbol information
    /// </summary>
    public class Symbols
    {
        internal static ConcurrentDictionary<string, ConcurrentDictionary<Symbols, byte>> Instances = new ConcurrentDictionary<string, ConcurrentDictionary<Symbols, byte>>();

        internal static void AddInstance(string server, Symbols symbols)
        {
            if (server == null)
                return;
            if (symbols == null)
                return;
            Instances.TryAdd(server, new ConcurrentDictionary<Symbols, byte>());
            if (Instances.TryGetValue(server, out var symbolsDic))
            {
                foreach (var item in symbolsDic.Keys)
                {
                    if (AreEqual(item, symbols))
                    {
                        symbols.SymGroups = item.SymGroups;
                        symbols.Sessions = item.Sessions;
                        symbols.Groups = item.Groups;
                        symbols.Infos = item.Infos;
                        symbols.InfosById = item.InfosById;
                        GC.Collect();
                        break;
                    }
                }
                symbolsDic.TryAdd(symbols, 0);
            }
        }

        internal static void RemoveInstance(string server, Symbols symbols)
        {
            if (server == null)
                return;
            if (symbols == null)
                return;
            if (Instances.TryGetValue(server, out var symbolsDic))
                symbolsDic.TryRemove(symbols, out _);
        }


        public SymBaseInfo Base;
        public SymGroup[] SymGroups;
        public ConcurrentDictionary<string, SymbolSessions> Sessions;
        public ConcurrentDictionary<string, SymGroup> Groups;
        public ConcurrentDictionary<string, SymbolInfo> Infos;
        public ConcurrentDictionary<int, SymbolInfo> InfosById;
        private MT5API Api;

        public Symbols(MT5API api)
        {
            Api = api;
        }

        public string[] Names
        {
            get
            {
                return Infos.Keys.ToArray();
            }
        }
        public string[] GroupNames
        {
            get
            {
                var list = new LinkedList<string>();
                foreach (var item in Groups.Values.ToArray())
                    list.AddLast(item.GroupName);
                return list.ToArray();
            }
        }

        public ComissionInfo[] Comissions { get; internal set; }

        public SymGroup GetGroupByName(string groupName)
        {
            if (Groups == null)
                throw new ConnectException();
            if (!Groups.ContainsKey(groupName))
                throw new Exception("Group not found: " + groupName);
            return Groups[groupName];
        }

        public SymbolInfo GetInfo(string symbol)
        {
            var infos = Infos;
            if (infos == null)
                Api.Connect();
            if (infos.Count == 0)
                Api.Connect();
            infos = Infos;
            if (infos.TryGetValue(symbol, out var res))
                return res;
            throw new Exception("Symbol not found: " + symbol);
        }

        ConcurrentDictionary<string, SymGroup> GetGroupHistory = new ConcurrentDictionary<string, SymGroup>();

        public SymGroup GetGroup(string symbol)
        {
            var groups = Groups;
            if (groups == null)
                throw new ConnectException();
            if (groups.Count == 0)
                throw new ConnectException("Not connected");
            if (GetGroupHistory.TryGetValue(symbol, out var gr))
                return gr;
            if (groups.TryGetValue(symbol, out var res))
            {
                var symGroups = SymGroups;
                if (symGroups != null)
                    foreach (var slave in symGroups)
                    {
                        if (slave.GroupName.Contains("(") || slave.GroupName.Contains(")"))
                            continue;
                        if (slave.GroupName.Contains("\\") && res.GroupName.Contains("\\"))
                        {
                            var sl = slave.GroupName.Substring(0, slave.GroupName.IndexOf("\\"));
                            var rs = res.GroupName.Substring(0, res.GroupName.IndexOf("\\"));
                            if (!sl.Contains("*") && !rs.Contains("*"))
                                if (sl != rs)
                                    continue;
                        }
                        foreach (var pattern in slave.GroupName.Replace(@"\", @"/").Replace(@".", @"\.").Replace(@"+", @"\+").Replace("*", ".*").Split(','))
                        {
                            if (!string.IsNullOrEmpty(pattern))
                                if (new Regex(pattern).Matches(res.GroupName.Replace(@"\", @"/")).Count > 0)
                                    res.CopyValues(slave);
                        }
                    }
                // ✅ Normalize NaN or infinity to 0
                if (double.IsNaN(res.MaxLots) || double.IsInfinity(res.MaxLots))
                    res.MaxVolume = 0;
                if (double.IsNaN(res.MinLots) || double.IsInfinity(res.MinLots))
                    res.MinVolume = 0;
                if (double.IsNaN(res.LotsStep) || double.IsInfinity(res.LotsStep))
                    res.VolumeStep = 0;
                GetGroupHistory[symbol] = res;
                return res;
            }
            else
                throw new Exception("Symbol not found: " + symbol);
        }

        public SymbolInfo GetInfo(int id)
        {
            var infos = InfosById;
            if (infos == null)
                throw new ConnectException();
            if (infos.TryGetValue(id, out var res))
                return res;
            else
                throw new Exception("Symbol not found: " + id);
        }

        public bool Exist(string symbol)
        {
            var infos = Infos;
            if (infos == null)
                throw new ConnectException();
            return infos.ContainsKey(symbol);
        }

        public string ExistStartsWith(string symbol)
        {
            var infos = Infos;
            if (infos == null)
                throw new ConnectException();
            foreach (var item in infos.Values)
                if (item.Currency.StartsWith(symbol))
                    return item.Currency;
            return null;
        }

        /// <summary>
        /// Returns commision information or null if no comission found for specified symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="ConnectException"></exception>
        /// <exception cref="Exception"></exception>
        public ComissionInfo GetComission(string symbol)
        {
            var groups = Groups;
            if (groups == null)
                throw new ConnectException();
            if (groups.TryGetValue(symbol, out var res))
            {
                var symbolGroupName = res.GroupName;
                var comissons = Comissions;
                if (comissons != null)
                    foreach (var item in comissons)
                        if (new Regex(item.GroupName.Replace(@".", @"\.").Replace(@"\", @"\\").Replace("*", ".*")).Matches(symbolGroupName).Count > 0)
                            return item;
                return null;
            }
            else
                throw new Exception("Symbol not found: " + symbol);
        }

        public static bool AreEqual(Symbols s1, Symbols s2)
        {
            
            // Compare the SymGroups array
            if (s1.SymGroups == null || s2.SymGroups == null)
            {
                if (s1.SymGroups != s2.SymGroups)
                    return false;
            }
            else if (!s1.SymGroups.SequenceEqual(s2.SymGroups))
                return false;

            if (s1.Sessions == null || s2.Sessions == null)
            {
                if (s1.Sessions != s2.Sessions)
                    return false;
            }
            else if (!s1.Sessions.SequenceEqual(s2.Sessions))
                return false;

            if (s1.Groups == null || s2.Groups == null)
            {
                if (s1.Groups != s2.Groups)
                    return false;
            }
            else if (!s1.Groups.SequenceEqual(s2.Groups))
                return false;

            if (s1.Infos == null || s2.Infos == null)
            {
                if (s1.Infos != s2.Infos)
                    return false;
            }
            else if (!s1.Infos.SequenceEqual(s2.Infos))
                return false;
            return true;
        }
    }
}
