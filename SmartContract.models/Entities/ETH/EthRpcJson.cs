using System;
using Newtonsoft.Json;
using SmartContract.Commons.Helpers;

namespace SmartContract.models.Entities.ETH
{
    public class EthRpcJson
    {
        public class Sender
        {
            public Sender()
            {
            }

            public Sender(string id, string method)
            {
                JsonRpc = "2.0";
                Id = id;
                Method = method;
            }

            [JsonProperty("jsonrpc")] public string JsonRpc;
            [JsonProperty("method")] public string Method;
            [JsonProperty("param")] public Object[] Param;
            [JsonProperty("id")] public string Id;

            public string GetJSon()
            {
                string output = JsonHelper.SerializeObject(this);
                output = output.Replace("param", "params");
                return output;
            }
        }

        public class Getter
        {
            public Getter(string input)
            {
                if (input == null)
                    return;
                Getter thing = JsonHelper.DeserializeObject<Getter>(input);
                Id = thing.Id;
                JsonRpc = thing.JsonRpc;
                Result = thing.Result;
            }

            [JsonProperty("id")] public string Id;
            [JsonProperty("jsonrpc")] public string JsonRpc;
            [JsonProperty("result")] public Object Result;
        }

        public class TransactionInfor
        {
            [JsonProperty("from")] public string From;
            [JsonProperty("to")] public string To;
            [JsonProperty("value")] public string Value;
            [JsonProperty("hash")] public string Hash;
            [JsonProperty("blockHash")] public string BlockHash;
            [JsonProperty("blockNumber")] public string BlockNumber;
            [JsonProperty("transactionIndex")] public string TransactionIndex;
            [JsonProperty("gas")] public string Gas;
            [JsonProperty("gasPrice")] public string GasPrice;
            [JsonProperty("input")] public string Input;
            [JsonProperty("nonce")] public string Nonce;
        }
    }
}