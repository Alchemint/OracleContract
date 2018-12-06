using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Helper = Neo.SmartContract.Framework.Helper;
using System;
using System.Numerics;
using Neo.SmartContract.Framework.Services.System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

namespace OracleContract
{
    public class OracleContract : SmartContract
    {
        private static readonly int EVENT_TYPE_SET_TYPEA = 1;
        private static readonly int EVENT_TYPE_SET_TYPEB = 2;
        private static readonly int EVENT_TYPE_SET_ACCOUNT = 3;
        private static readonly int EVENT_TYPE_SET_ADDR = 4;
        private static readonly int EVENT_TYPE_SET_MEDIAN = 5;
        private static readonly int EVENT_TYPE_REMOVE_ADDR = 6;

        [DisplayName("oracleOperator")]
        public static event Action<byte[], byte[], byte[], BigInteger, int> Operated;

        //Static param
        private const string COMMITTEE_ACCOUNT = "committee_account";
        private const string ADMIN_ACCOUNT = "admin_account";

        //Default multiple signature committee account
        private static readonly byte[] committee = Helper.ToScriptHash("AQdP56hHfo54JCWfpPw4MXviJDtQJMtXFa");
        //Default admin account,autorized by committee account
        private static readonly byte[] admin = Helper.ToScriptHash("AQdP56hHfo54JCWfpPw4MXviJDtQJMtXFa");

        /*     
        * Key wrapper
        */
        private static byte[] GetTypeAParaKey(byte[] account) => new byte[] { 0x01 }.Concat(account);
        private static byte[] GetTypeAKey(string strKey) => new byte[] { 0x02 }.Concat(strKey.AsByteArray());
        private static byte[] GetTypeBKey(string key) => new byte[] { 0x03 }.Concat(key.AsByteArray());
        private static byte[] GetParaAddrKey(string paraKey, byte[] addr) => new byte[] { 0x10 }.Concat(paraKey.AsByteArray().Concat(addr));
        private static byte[] GetMedianKey(string key) => new byte[] { 0x20 }.Concat(key.AsByteArray());
        private static byte[] GetConfigKey(byte[] key) => new byte[] { 0x30 }.Concat(key);
        private static byte[] GetAccountKey(string key) => new byte[] { 0x40 }.Concat(key.AsByteArray());

        public static Object Main(string operation, params object[] args)
        {
            var magicstr = "2018-11-30 10:40";

            //only admin account can call this method
            if (operation == "setTypeA")
            {
                if (args.Length != 2) return false;

                string key = (string)args[0];

                BigInteger value = (BigInteger)args[1];

                return setTypeA(key, value);
            }

            if (operation == "getTypeA")
            {
                if (args.Length != 1) return false;

                string key = (string)args[0];

                return getTypeA(key);
            }

            //only committee account can call this method
            if (operation == "setAccount")
            {
                if (args.Length != 2) return false;

                string key = (string)args[0];
                byte[] address = (byte[])args[1];

                if (!checkCommittee()) return false;

                return setAccount(key, address);
            }

            if (operation == "getAccount")
            {
                if (args.Length != 1) return false;
                string key = (string)args[0];

                return Storage.Get(Storage.CurrentContext, GetAccountKey(key));
            }

            //only admin account can call this method
            if (operation == "addParaAddrWhit")
            {
                if (args.Length != 3) return false;

                string para = (string)args[0];

                byte[] addr = (byte[])args[1];

                BigInteger state = (BigInteger)args[2];

                return addParaAddrWhit(para, addr, state);
            }

            //only admin account can call this method
            if (operation == "removeParaAddrWhit")
            {
                if (args.Length != 2) return false;

                string para = (string)args[0];

                byte[] addr = (byte[])args[1];

                return removeParaAddrWhit(para, addr);
            }

            if (operation == "getApprovedAddrs")
            {
                if (args.Length != 1) return false;

                string para = (string)args[0];

                byte[] prefix = GetParaAddrKey(para, new byte[] { });

                return getDataWithPrefix(prefix);
            }

            if (operation == "getAddrWithParas")
            {
                if (args.Length != 1) return false;

                string para = (string)args[0];

                byte[] prefix = new byte[] { };

                return getDataWithPara(para);
            }

            //only authorized account can call this method
            if (operation == "setTypeB")
            {
                if (args.Length != 3) return false;

                string para = (string)args[0];

                byte[] from = (byte[])args[1];

                BigInteger value = (BigInteger)args[2];

                BigInteger state = (BigInteger)Storage.Get(Storage.CurrentContext, GetParaAddrKey(para, from)).AsBigInteger();

                if (state == 0) return false;

                return setTypeB(para, from, value);
            }

            if (operation == "getTypeB")
            {
                if (args.Length != 1) return false;
                string key = (string)args[0];

                return getTypeB(key);
            }

            if (operation == "getStructConfig")
            {
                return getStructConfig();
            }

            //only admin account can call this method
            if (operation == "setStructConfig")
            {
                if (!checkAdmin()) return false;
                return setStructConfig();
            }

            #region contract upgrade
            if (operation == "upgrade")
            {
                if (!checkCommittee()) return false;

                if (args.Length != 1 && args.Length != 9)
                    return false;

                byte[] script = Blockchain.GetContract(ExecutionEngine.ExecutingScriptHash).Script;
                byte[] new_script = (byte[])args[0];

                if (script == new_script)
                    return false;

                byte[] parameter_list = new byte[] { 0x07, 0x10 };
                byte return_type = 0x05;
                //1|0|4
                bool need_storage = (bool)(object)05;
                string name = "datacenter";
                string version = "1";
                string author = "alchemint";
                string email = "0";
                string description = "alchemint";

                if (args.Length == 9)
                {
                    parameter_list = (byte[])args[1];
                    return_type = (byte)args[2];
                    need_storage = (bool)args[3];
                    name = (string)args[4];
                    version = (string)args[5];
                    author = (string)args[6];
                    email = (string)args[7];
                    description = (string)args[8];
                }
                Contract.Migrate(new_script, parameter_list, return_type, need_storage, name, version, author, email, description);
                return true;
            }
            #endregion

            return true;
        }

        private static bool checkAdmin()
        {
            byte[] currAdmin = Storage.Get(Storage.CurrentContext, GetAccountKey(ADMIN_ACCOUNT));

            if (currAdmin.Length > 0)
            {
                if (!Runtime.CheckWitness(currAdmin)) return false;
            }
            else
            {
                if (!Runtime.CheckWitness(admin)) return false;
            }
            return true;
        }

        private static bool checkCommittee()
        {
            byte[] currCommittee = Storage.Get(Storage.CurrentContext, GetAccountKey(COMMITTEE_ACCOUNT));

            if (currCommittee.Length > 0)
            {
                if (!Runtime.CheckWitness(currCommittee)) return false;
            }
            else
            {
                if (!Runtime.CheckWitness(committee)) return false;
            }
            return true;
        }

        public static bool setAccount(string key, byte[] address)
        {
            if (address.Length != 20)
                throw new InvalidOperationException("The parameters address and to SHOULD be 20-byte addresses.");

            Storage.Put(Storage.CurrentContext, GetAccountKey(key), address);

            Operated(address, key.AsByteArray(), null, 0, EVENT_TYPE_SET_ACCOUNT);

            return true;
        }

        public static bool addParaAddrWhit(string para, byte[] addr, BigInteger state)
        {
            if (!checkAdmin()) return false;

            if (addr.Length != 20) return false;

            byte[] byteKey = GetParaAddrKey(para, addr);

            if (Storage.Get(Storage.CurrentContext, byteKey).AsBigInteger() != 0 || state == 0) return false;

            Storage.Put(Storage.CurrentContext, byteKey, state);

            Operated(addr, para.AsByteArray(), null, state, EVENT_TYPE_SET_ADDR);

            return true;
        }

        public static bool removeParaAddrWhit(string para, byte[] addr)
        {
            if (!checkAdmin()) return false;

            byte[] paraAddrByteKey = GetParaAddrKey(para, addr);

            Storage.Delete(Storage.CurrentContext, paraAddrByteKey);

            Map<byte[], BigInteger> map = getObjectWithKey(para);

            map.Remove(addr);

            Storage.Put(Storage.CurrentContext, GetTypeBKey(para), map.Serialize());

            Operated(addr, para.AsByteArray(), null, 0, EVENT_TYPE_REMOVE_ADDR);

            return true;
        }

        public static Map<byte[], BigInteger> getObjectWithKey(string key)
        {
            byte[] data = Storage.Get(Storage.CurrentContext, GetTypeBKey(key));

            Map<byte[], BigInteger> map = new Map<byte[], BigInteger>();

            if (data.Length > 0)

                map = data.Deserialize() as Map<byte[], BigInteger>;

            return map;
        }

        public static Object getDataWithPara(string para)
        {

            Map<byte[], BigInteger> map = getObjectWithKey(para);

            var array = new Object[map.Values.Length];

            int index = 0;

            foreach (byte[] addr in map.Keys)
            {
                NodeObj obj = new NodeObj();

                obj.value = map[addr];

                obj.addr = addr;

                array[index] = obj;

                index++;
            }


            return array;
        }

        public static Object getDataWithPrefix(byte[] prefix)
        {
            int count = 0;

            Map<byte[], BigInteger> map = new Map<byte[], BigInteger>();

            Iterator<byte[], byte[]> iterator = Storage.Find(Storage.CurrentContext, prefix);

            while (iterator.Next())
            {
                if (iterator.Key.Range(0, prefix.Length) == prefix)
                {
                    count++;

                    byte[] rawKey = iterator.Key;

                    byte[] addr = rawKey.Range(prefix.Length, rawKey.Length - prefix.Length);

                    BigInteger value = iterator.Value.AsBigInteger();

                    map[addr] = value;
                }
            }

            var objs = new Object[count];

            if (count == 0) return objs;

            int index = 0;

            foreach (byte[] addr in map.Keys)
            {
                NodeObj obj = new NodeObj();
                obj.addr = addr;
                obj.value = map[addr];

                objs[index] = obj;

                index++;
            }

            return objs;

        }

        public static bool setTypeA(string key, BigInteger value)
        {
            if (key == null || key == "") return false;

            if (!checkAdmin()) return false;

            byte[] byteKey = GetTypeAKey(key);

            Storage.Put(Storage.CurrentContext, byteKey, value);

            byte[] currAdmin = Storage.Get(Storage.CurrentContext, GetAccountKey(ADMIN_ACCOUNT));

            if (currAdmin.Length == 0) currAdmin = admin;

            Operated(currAdmin, key.AsByteArray(), null, value, EVENT_TYPE_SET_TYPEA);
            return true;
        }

        public static BigInteger getTypeA(string key)
        {
            byte[] byteKey = GetTypeAKey(key);

            BigInteger value = Storage.Get(Storage.CurrentContext, byteKey).AsBigInteger();

            return value;
        }


        public static bool setTypeB(string key, byte[] addr, BigInteger value)
        {
            if (key == null || key == "") return false;

            if (value <= 0) return false;

            if (!Runtime.CheckWitness(addr)) return false;

            Map<byte[], BigInteger> map = getObjectWithKey(key);

            map[addr] = value;

            Storage.Put(Storage.CurrentContext, GetTypeBKey(key), map.Serialize());

            Operated(addr, key.AsByteArray(), null, value, EVENT_TYPE_SET_TYPEB);

            BigInteger medianValue = computeMedian(key);

            Operated(addr, key.AsByteArray(), null, medianValue, EVENT_TYPE_SET_MEDIAN);

            return true;
        }

        public static BigInteger getTypeB(string key)
        {
            return getMedian(key);
        }

        public static Config getStructConfig()
        {
            byte[] value = Storage.Get(Storage.CurrentContext, GetConfigKey("structConfig".AsByteArray()));
            if (value.Length > 0)
                return Helper.Deserialize(value) as Config;
            return new Config();
        }

        public static bool setStructConfig()
        {
            Config config = new Config();

            config.liquidate_line_rate_b = getTypeA("liquidate_line_rate_b"); //50
            config.liquidate_line_rate_c = getTypeA("liquidate_line_rate_c"); //150

            config.debt_top_c = getTypeA("debt_top_c"); //1000000000000;

            config.issuing_fee_b = getTypeA("issuing_fee_b"); //1000000000;
            config.liquidate_top_rate_c = getTypeA("liquidate_top_rate_c");// 160;

            config.liquidate_dis_rate_c = getTypeA("liquidate_dis_rate_c"); // 90;
            config.liquidate_line_rateT_c = getTypeA("liquidate_line_rateT_c"); // 120; 

            config.fee_rate_c = getTypeA("fee_rate_c"); //148;

            Storage.Put(Storage.CurrentContext, GetConfigKey("structConfig".AsByteArray()), Helper.Serialize(config));

            return true;
        }

        public static BigInteger getMedian(string key)
        {
            return Storage.Get(Storage.CurrentContext, GetMedianKey(key)).AsBigInteger();
        }

        public static BigInteger computeMedian(string key)
        {
            Map<byte[], BigInteger> map = getObjectWithKey(key);

            var prices = new BigInteger[map.Values.Length];
            {
                int index = 0;

                foreach (BigInteger val in map.Values)
                {
                    if (val > 0)
                    {
                        prices[index] = val;

                        index++;
                    }
                }
            }

            BigInteger temp;
            for (int i = 0; i < prices.Length; i++)
            {
                for (int j = i; j < prices.Length; j++)
                {
                    if (prices[i] > prices[j])
                    {
                        temp = prices[i];
                        prices[i] = prices[j];
                        prices[j] = temp;
                    }
                }
            }

            BigInteger value = 0;

            if (prices.Length > 0)
            {
                if (prices.Length % 2 != 0)
                {
                    value = prices[(prices.Length + 1) / 2 - 1];
                }
                else
                {
                    int index = prices.Length / 2;

                    value = (prices[index] + prices[index - 1]) / 2;
                }

                Storage.Put(Storage.CurrentContext, GetMedianKey(key), value);
            }


            return value;
        }



        public class Config
        {
            public BigInteger liquidate_line_rate_b;

            public BigInteger liquidate_line_rate_c;

            public BigInteger liquidate_dis_rate_c;

            public BigInteger fee_rate_c;

            public BigInteger liquidate_top_rate_c;

            public BigInteger liquidate_line_rateT_c;

            public BigInteger issuing_fee_c;

            public BigInteger issuing_fee_b;

            public BigInteger debt_top_c;

        }

        public class NodeObj
        {
            public byte[] addr;

            public BigInteger value;
        }
    }
}
