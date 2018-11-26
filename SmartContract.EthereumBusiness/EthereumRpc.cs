using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts.CQS;
//using Nethereum.StandardTokenEIP20.CQS;
using Nethereum.Web3.Accounts;
using SmartContract.BlockchainBusiness;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Entities.ETH;
using Nethereum.StandardTokenEIP20.CQS;
using Nethereum.ENS;
using Nethereum.RPC.Eth.DTOs;

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
                ReturnObject result =
                    EthereumSendRPC(EthereumRpcList.RpcName.PersonalNewAccount, new Object[] { password });
                Console.WriteLine(result);
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
                        Data = getter.Result.ToString()
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
                // var senderAddress = AccountFactory.Address;
                //  var web3 = _ethereumClientIntegrationFixture.GetWeb3();
                var url = AppSettingHelper.GetEthereumNode();
                var privateKey = "08786bec5c4b2ac8cb6dcafb320a3486d59a0e9f81860792fe13ca0a962cda3e";
                var account = new Account(privateKey);
                var web3 = new Nethereum.Web3.Web3(account, url);
                //var deploymentMessage = new StandardTokenDeployment
                //{
                //    TotalSupply = 10000,
                //    FromAddress = blockchainTransaction.FromAddress,
                //    Gas = new HexBigInteger(900000)
                //};

                //  var deploymentHandler = web3.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
                //   var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);

                var contractAddress = "0xB06C9F6A829fAEd77F227629586C982c37B7b408";
                var newAddress = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";



                var transactionMessage = new TransferFunction
                {
                    FromAddress = blockchainTransaction.FromAddress,
                    To = newAddress,
                    AmountToSend = 1000
                };

                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var transferReceipt =
                    await transferHandler.SendRequestAndWaitForReceiptAsync(transactionMessage, contractAddress);
                //  var transferEventOutput = transferReceipt.
                //   var transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transferReceipt.TransactionHash);
                Console.WriteLine(JsonHelper.SerializeObject(transferReceipt));
                //   var transferDecoded = transaction.DecodeTransactionToFunctionMessage<TransferFunction>();

                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,
                    Message = ""
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