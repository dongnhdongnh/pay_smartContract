using System.Collections.Generic;
using SmartContract.models.Entities.ETH;

namespace SmartContract.EthereumBusiness
{
     public class EthereumRpcList
    {
        static Dictionary<RpcName, EthRpcJson.Sender> _datas;

        static Dictionary<RpcName, EthRpcJson.Sender> Datas
        {
            get
            {
                if (_datas == null)
                {
                    _datas = new Dictionary<RpcName, EthRpcJson.Sender>();
                    _datas[RpcName.EthBlockNumber] = new EthRpcJson.Sender("1", "eth_blockNumber");
                    _datas[RpcName.EthAccounts] = new EthRpcJson.Sender("1", "eth_accounts");
                    _datas[RpcName.EthGetBlockByNumber] = new EthRpcJson.Sender("1", "eth_getBlockByNumber");
                    _datas[RpcName.EthGetBlockByHash] = new EthRpcJson.Sender("1", "eth_getBlockByHash");
                    _datas[RpcName.PersonalNewAccount] = new EthRpcJson.Sender("74", "personal_newAccount");
                    _datas[RpcName.PersonalSendTransaction] = new EthRpcJson.Sender("1", "personal_sendTransaction");
                    _datas[RpcName.EthGetTransactionByHash] = new EthRpcJson.Sender("1", "eth_getTransactionByHash");
                    _datas[RpcName.EthGetTransactionByBlockNumberAndIndex] =
                        new EthRpcJson.Sender("1", "eth_getTransactionByBlockNumberAndIndex");
                    _datas[RpcName.EthSendTransaction] = new EthRpcJson.Sender("1", "eth_sendTransaction");
                    _datas[RpcName.EthGetBalance] = new EthRpcJson.Sender("1", "eth_getBalance");
                    _datas[RpcName.EthSendDrawTransaction] = new EthRpcJson.Sender("1", "eth_sendRawTransaction");
                    _datas[RpcName.EthGetTransactionReceipt] = new EthRpcJson.Sender("1", "eth_getTransactionReceipt");
                }

                return _datas;
            }
        }

        public enum RpcName
        {
            EthAccounts,
            EthBlockNumber,
            EthGetBlockByNumber,
            EthGetBalance,
            EthGetBlockByHash,
            PersonalNewAccount,
            PersonalSendTransaction,
            EthGetTransactionByHash,
            EthSendDrawTransaction,
            EthGetTransactionReceipt,
            EthGetTransactionByBlockNumberAndIndex,
            EthSendTransaction
        }
        //private readonly EthRPCJson.Sender Eth_accounts()
        //	{

        //	}

        public static EthRpcJson.Sender GetSender(RpcName rpcName)
        {
            return Datas[rpcName];
        }
    }
}