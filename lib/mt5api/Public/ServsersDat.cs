using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Server name and adresses from servers.dat
    /// </summary>
    public class Access
    {
        public AccessRec AccessRec;
        public AddressRec[] Addresses;

        public Access Clone()
        {
            var clone = new Access();

            // Deep copy AccessRec
            clone.AccessRec = new AccessRec
            {
                ServerName = this.AccessRec.ServerName,
                // Clone byte arrays
                s40 = (byte[])this.AccessRec.s40.Clone(),
                sC0 = this.AccessRec.sC0,
                sC4 = this.AccessRec.sC4,
                sC8 = (byte[])this.AccessRec.sC8.Clone()
            };

            // Deep copy AddressRec[]
            if (this.Addresses != null)
            {
                clone.Addresses = new AddressRec[this.Addresses.Length];
                for (int i = 0; i < this.Addresses.Length; i++)
                {
                    var src = this.Addresses[i];
                    clone.Addresses[i] = new AddressRec
                    {
                        Address = src.Address,
                        s80 = src.s80,
                        s84 = src.s84,
                        s88 = src.s88,
                        s8C = src.s8C,
                        s90 = src.s90
                    };
                }
            }

            return clone;
        }
    }
    /// <summary>
    /// Server name and adresses from servers.dat
    /// </summary>
    public class AccessEx
    {
        public AccessRecEx AccessRec;
        public AddressRecEx[] Addresses;
    }
    /// <summary>
    /// Server details from servers.dat
    /// </summary>
    public class Server
    {
        public ServerInfoEx ServerInfoEx;
        public ServerInfo ServerInfo;
        public Access[] Accesses;
        public AccessEx[] AccessesEx;
    }

    internal class ServersDatLoader
    {
        internal Server[] Load(string path)
        {
            return Load(File.ReadAllBytes(path));
        }

        internal Server[] Load(byte[] bytes)
        {
            InBuf buf = new InBuf(bytes, 0);
            var hdr = buf.Struct<DatHeader>();
            List<Server> lst = new List<Server>();
            while (true)
            {
                if (buf.Left == 0)
                    break;
                Server res = new Server();
                if (hdr.Id == 0x1F9 || hdr.Id == 0x1FA)
                {
                    res.ServerInfoEx = buf.CryptStruct<ServerInfoEx>(ServerInfoEx.Size);
                    int num = buf.Int();
                    if (num < 0 || num > 128)
                        break;
                    res.Accesses = new Access[num];
                    for (int i = 0; i < num; i++)
                    {
                        res.Accesses[i] = new Access();
                        res.Accesses[i].AccessRec = buf.CryptStruct<AccessRec>(AccessRec.Size);
                        res.Accesses[i].Addresses = buf.CryptArray<AddressRec>(AddressRec.Size);
                    }
                    num = buf.Int();
                    if (num < 0 || num > 128)
                        break;
                    res.AccessesEx = new AccessEx[num];
                    for (int i = 0; i < num; i++)
                    {
                        res.AccessesEx[i] = new AccessEx();
                        res.AccessesEx[i].AccessRec = buf.CryptStruct<AccessRecEx>(AccessRecEx.Size);
                        res.AccessesEx[i].Addresses = buf.CryptArray<AddressRecEx>(AddressRecEx.Size);
                    }
                }
                else if (hdr.Id == 0x1F7 || hdr.Id == 0x1F8)
                {
                    res.ServerInfo = buf.CryptStruct<ServerInfo>(ServerInfo.Size);
                    //buf.Bytes(8);
                    int num = buf.Int();
                    if (num < 0 || num > 128)
                        break;
                    res.Accesses = new Access[num];
                    for (int i = 0; i < num; i++)
                    {
                        res.Accesses[i] = new Access();
                        res.Accesses[i].AccessRec = buf.CryptStruct<AccessRec>(AccessRec.Size);
                        res.Accesses[i].Addresses = buf.CryptArray<AddressRec>(AddressRec.Size);
                    }
                }
                else
                    throw new NotImplementedException("hdr.Id = 0x" + hdr.Id.ToString("X"));
                lst.Add(res);
            }
            return lst.ToArray();
        }
        

    }


    internal interface ToBufWriter
    {
        void WriteToBuf(OutBuf buf);
    }

    public class ServersDatSaver
    {
        internal void Save(string path, Server[] servers, DatHeader header)
        {
            var bytes = Save(servers, header);
            File.WriteAllBytes(path, bytes);
        }

        internal byte[] Save(Server[] servers, DatHeader header)
        {
            var buf = new OutBuf();
            buf.Add(UDTSaver.WriteStruct(header, DatHeader.Size));

            foreach (var server in servers)
            {
                if (header.Id == 0x1F9 || header.Id == 0x1FA)
                {
                    buf.Add(UDTSaver.WriteCryptStruct(server.ServerInfoEx, ServerInfoEx.Size));

                    buf.Add(server.Accesses.Length);
                    foreach (var access in server.Accesses)
                    {
                        buf.Add(UDTSaver.WriteCryptStruct(access.AccessRec, AccessRec.Size));
                        WriteCryptArray(buf, access.Addresses, AddressRec.Size);
                    }

                    buf.Add(server.AccessesEx.Length);
                    foreach (var accessEx in server.AccessesEx)
                    {
                        buf.Add(UDTSaver.WriteCryptStruct(accessEx.AccessRec, AccessRecEx.Size));
                        WriteCryptArray(buf, accessEx.Addresses, AddressRecEx.Size);
                    }
                }
                else if (header.Id == 0x1F7 || header.Id == 0x1F8)
                {
                    buf.Add(UDTSaver.WriteCryptStruct(server.ServerInfo, ServerInfo.Size));

                    buf.Add(server.Accesses.Length);
                    foreach (var access in server.Accesses)
                    {
                        buf.Add(UDTSaver.WriteCryptStruct(access.AccessRec, AccessRec.Size));
                        WriteCryptArray(buf, access.Addresses, AddressRec.Size);
                    }
                }
                else
                {
                    throw new NotImplementedException("header.Id = 0x" + header.Id.ToString("X"));
                }
            }

            return buf.ToArray();
        }

        private void WriteCryptArray<T>(OutBuf buf, T[] array, int structSize) where T : ToBufWriter, new()
        {
            buf.Add(array.Length);
            foreach (var item in array)
                buf.Add(UDTSaver.WriteCryptStruct(item, structSize));
        }

       
    }

    internal static class UDTSaver
    {
        public static byte[] WriteCryptStruct<T>(T obj, int size) where T : ToBufWriter
        {
            var tempBuf = new OutBuf();
            obj.WriteToBuf(tempBuf);
            var bytes = tempBuf.ToArray();
            Crypt.EasyCrypt(bytes);
            if (bytes.Length != size)
                throw new Exception($"Size mismatch: expected {size}, got {bytes.Length}");
            return bytes;
        }

        public static byte[] WriteStruct<T>(T obj, int size) where T : ToBufWriter
        {
            var tempBuf = new OutBuf();
            obj.WriteToBuf(tempBuf);
            var bytes = tempBuf.ToArray();

            if (bytes.Length != size)
                throw new Exception($"Size mismatch: expected {size}, got {bytes.Length}");

            return bytes;
        }
    }
}
//3472