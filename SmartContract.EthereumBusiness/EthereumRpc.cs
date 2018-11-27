using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.StandardTokenEIP20.CQS;
using Nethereum.Web3.Accounts;
using SmartContract.BlockchainBusiness;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Entities.ETH;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;


namespace SmartContract.EthereumBusiness
{
    public class EthereumRpc : IBlockchainRpc
    {
        public string EndPointUrl { get; set; }
        private static string rootAddress = "";
        private static string rootPassword = "";

        public static void SetAdminAddressPassword(string inputRootAddress, string inputRootPassword)
        {
            rootAddress = inputRootAddress;
            rootPassword = inputRootPassword;
        }

        public EthereumRpc(string url)
        {
            EndPointUrl = url;
        }

        /// <summary>
        /// Send RPC to get JSON DAta
        /// </summary>
        /// <param name="rpcName">name of RPC method</param>
        /// <param name="ps">params send to RPC</param>
        /// <returns></returns>
        public ReturnObject EthereumSendRPC(EthereumRpcList.RpcName rpcName, Object[] ps = null)
        {
            try
            {
                //Console.WriteLine("=====================" + RpcName + "=======================");
                // Set a default policy level for the "http:" and "https" schemes.
                HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Default);
                HttpWebRequest.DefaultCachePolicy = policy;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(EndPointUrl);
                // Define a cache policy for this request only. 
                HttpRequestCachePolicy noCachePolicy =
                    new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                httpWebRequest.CachePolicy = noCachePolicy;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";


                //};
                EthRpcJson.Sender sender = EthereumRpcList.GetSender(rpcName);
                if (ps != null)
                    sender.Param = ps;
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = sender.GetJSon();
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    return new ReturnObject
                    {
                        Status = Status.STATUS_COMPLETED,
                        Data = result,
                    };
                }
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        /// <summary>
        /// Send Transaction with  Passphrase
        /// </summary>
        /// <param name="from"></param>
        /// <param name="toAddress"></param>
        /// <param name="amount"></param>
        /// <param name="passphrase"></param>
        /// <returns></returns>
        public ReturnObject SendTransactionWithPassphrase(string from, string toAddress, decimal amount,
            string passphrase)
        {
            try
            {
                decimal weiAmount = EtherToWei(amount);
                EthRpcJson.TransactionInfor sender = new EthRpcJson.TransactionInfor()
                {
                    From = from,
                    To = toAddress,
                    Value = ((BigInteger)weiAmount).ToHex()
                };

                //var tx = { from: "0x391694e7e0b0cce554cb130d723a9d27458f9298", to: "0xafa3f8684e54059998bc3a7b0d2b0da075154d66", value: web3.toWei(1.23, "ether")};
                var result = EthereumSendRPC(EthereumRpcList.RpcName.PersonalSendTransaction,
                    new Object[] { sender, passphrase });
                if (result.Status == Status.STATUS_ERROR)
                {
                    return result;
                }

                EthRpcJson.Getter getter = JsonHelper.DeserializeObject<EthRpcJson.Getter>(result.Data);
                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,
                    Data = getter.Result.ToString()
                };
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        private static decimal EtherToWei(decimal amount)
        {
            return amount * 1000000000000000000;
        }

        public static decimal WeiToEther(BigInteger amount)
        {
            return ((decimal)amount) / 1000000000000000000;
        }

        /// <summary>
        /// This function send transaction
        /// </summary>
        /// <param name="from"></param>
        /// <param name="toAddress"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public ReturnObject SendTransaction(string from, string toAddress, decimal amount)
        {
            EthRpcJson.TransactionInfor sender = new EthRpcJson.TransactionInfor()
            {
                From = from,
                To = toAddress,
                Value = ((int)amount).IntToHex()
            };
            return EthereumSendRPC(EthereumRpcList.RpcName.EthSendTransaction, new Object[] { sender });
        }


        public ReturnObject FindTransactionByBlockNumberAndIndex(int blockNumber, int transactionIndex)
        {
            return EthereumSendRPC(EthereumRpcList.RpcName.EthGetTransactionByBlockNumberAndIndex,
                new Object[] { blockNumber.IntToHex(), transactionIndex.IntToHex() });
        }


        public ReturnObject CreateNewAddress(string password)
        {
            try
            {
                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
                var account = new Account(privateKey);


                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,
                    Data = account.Address
                };
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        public ReturnObject CreateNewAddress()
        {
            throw new NotImplementedException();
        }

        public ReturnObject CreateNewAddress(string privateKey, string publicKey)
        {
            throw new NotImplementedException();
        }

        public ReturnObject SendRawTransaction(string data)
        {
            throw new NotImplementedException();
        }

        public ReturnObject GetBalance(string address)
        {
            throw new NotImplementedException();
        }

        public ReturnObject SignTransaction(string privateKey, object[] transactionData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send Transaction Async
        /// </summary>
        /// <param name="blockchainTransaction"></param>
        /// <returns></returns>
        public async Task<ReturnObject> SendTransactionAsync(BlockchainTransaction blockchainTransaction)
        {
            try
            {
                var resultSend = SendTransactionWithPassphrase(rootAddress, blockchainTransaction.ToAddress,
                    blockchainTransaction.Amount, rootPassword);
                return resultSend;
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        public async Task<ReturnObject> SendTransactionAsync1(BlockchainTransaction blockchainTransaction)
        {
            try
            {
                var privateKey = "08786bec5c4b2ac8cb6dcafb320a3486d59a0e9f81860792fe13ca0a962cda3e";


                var senderAddress = "0xc2a213f481b2c794704218365e13d4761820b398";
                var contractAddress = "0x8070f3cba5833490b8794e2b4894b59a45cbbba8";
                var account = new Account(privateKey);
                var web3 = new Web3(account, "https://ropsten.infura.io/v3/e2bd8adca45547c38efb10566fa7eec1");

                var value = (BigInteger)EtherToWei(blockchainTransaction.Amount);
                var transactionMessage = new TransferFunction()
                {
                    FromAddress = senderAddress,
                    To = blockchainTransaction.ToAddress,
                    Value = value
                };

                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var transferReceipt =
                    await transferHandler.SendRequestAndWaitForReceiptAsync(transactionMessage, contractAddress);


                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,
                    Data = transferReceipt.TransactionHash
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }
        public async Task<ReturnObject> TestSmartContractFunction()
        {

            try
            {
                string abi = AppSettingHelper.GetSmartContractAbi();
                Nethereum.Contracts.Contract contract = Web3Api.GetContract(abi, AppSettingHelper.GetSmartContractAddress());
                Function funct = Web3Api.getFunction(contract, "balances");
                var result = await funct.CallAsync<BigInteger>("0xc942F1D286d9b8002206CbB3196f46Fa892aAD93");
                Console.WriteLine(result);
                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,

                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }


        public static async Task<TransactionReceipt> MineAndGetReceiptAsync(Web3 web3, string transactionHash)
        {
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            while (receipt == null)
            {
                Thread.Sleep(4000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            return receipt;
        }


        public ReturnObject GetBlockByNumber(int blockNumber)
        {
            try
            {
                ReturnObject result = EthereumSendRPC(EthereumRpcList.RpcName.EthGetBlockByNumber,
                    new Object[] { blockNumber.IntToHex(), true });
                //Console.WriteLine(_result);
                if (result.Status == Status.STATUS_ERROR)
                {
                    return result;
                }
                else
                {
                    //	Console.WriteLine();
                    EthRpcJson.Getter getter =
                        JsonHelper.DeserializeObject<EthRpcJson.Getter>(result.Data);
                    return new ReturnObject
                    {
                        Status = Status.STATUS_COMPLETED,
                        Data = JsonHelper.SerializeObject(getter.Result)
                    };
                }
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }
        }

        public async Task<ReturnObject> GetBlockByNumberAsyn(int blockNumber)
        {
            throw new NotImplementedException();
        }


        public ReturnObject FindTransactionByHash(string hash)
        {
            return EthereumSendRPC(EthereumRpcList.RpcName.EthGetTransactionByHash, new Object[] { hash });
        }

        public async Task<ReturnObject> FindTransactionByHashAsyn(string transactionHash)
        {
            throw new NotImplementedException();
        }

        public ReturnObject FindBlockByHash(string hash)
        {
            return EthereumSendRPC(EthereumRpcList.RpcName.EthGetBlockByHash, new Object[] { hash, true });
        }

        public ReturnObject FindBlockByNumber(int number)
        {
            return EthereumSendRPC(EthereumRpcList.RpcName.EthGetBlockByNumber,
                new Object[] { number.IntToHex(), true });
            //return null;
        }

        public ReturnObject GetAccounts()
        {
            return EthereumSendRPC(EthereumRpcList.RpcName.EthAccounts);
        }

        public ReturnObject GetBlockNumber()
        {
            try
            {
                var result = EthereumSendRPC(EthereumRpcList.RpcName.EthBlockNumber);
                if (result.Status == Status.STATUS_ERROR)
                {
                    return result;
                }
                else
                {
                    EthRpcJson.Getter getter =
                        JsonHelper.DeserializeObject<EthRpcJson.Getter>(result.Data);
                    int _blockNumber = -1;
                    if (!getter.Result.ToString().HexToInt(out _blockNumber))
                    {
                        throw new Exception("cant get int from hex");
                    }

                    return new ReturnObject
                    {
                        Status = Status.STATUS_COMPLETED,
                        Data = _blockNumber.ToString()
                    };
                }
            }
            catch (Exception e)
            {
                return new ReturnObject
                {
                    Status = Status.STATUS_ERROR,
                    Message = e.Message
                };
            }

            //return 
        }
    }



}