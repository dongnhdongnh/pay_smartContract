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
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Threading;
using Nethereum.Contracts;

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
                // var senderAddress = AccountFactory.Address;
                //  var web3 = _ethereumClientIntegrationFixture.GetWeb3();
                var abi = @"[{'constant':false,'inputs':[{'name':'_isAllowConvertExchange','type':'bool'}],'name':'setAllowConvertExchange','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[],'name':'setLockAYear','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_limitEndDateSecond','type':'uint256'}],'name':'setLimitEndDate','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'LOCK_WITH_EIGHT_WEEKS','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_spender','type':'address'},{'name':'_value','type':'uint256'}],'name':'approve','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'isLock','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_lockEndTimeSecond','type':'uint256'}],'name':'setLockEndTime','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'allowTransfers','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transferFrom','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'balances','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'LOCK_WITH_ONE_YEAR','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_rateExchange','type':'uint256'}],'name':'configRateExchange','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'limitedWallets','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'decimals','outputs':[{'name':'','type':'uint8'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'rateExchange','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'setLockTwelveWeeks','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_isAllowTransferDollar','type':'bool'}],'name':'setAllowTransferDollar','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'limitedWalletsManager','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'lockEndTime','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'issuanceFinished','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_value','type':'uint256'}],'name':'destroyDollar','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'},{'name':'','type':'address'}],'name':'allowed','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_isLock','type':'bool'}],'name':'setLock','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_spender','type':'address'},{'name':'_subtractedValue','type':'uint256'}],'name':'decreaseApproval','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'LIMIT_TRANSFERS_PERIOD','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'setLockFourWeeks','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_limitTimeTransferEndDateSecond','type':'uint256'}],'name':'setLimitTimeTransferEndDate','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'isAllowTransferDollar','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalIssue','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_isLimitTimeTransfer','type':'bool'}],'name':'setAllowLimitTimeTransfer','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_wallet','type':'address'}],'name':'delLimitedWalletAddress','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transferDollar','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'isLimitTimeTransfer','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_isLimitEnabled','type':'bool'}],'name':'setAllowLimitedWallet','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'issue','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'limitEndDate','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'owner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_isLimitWithTime','type':'bool'}],'name':'setAllowLimitedWalletWithTime','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'lockTotal','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_value','type':'uint256'}],'name':'destroy','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'LOCK_WITH_TWELVE_WEEKS','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transfer','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_listener','type':'address'}],'name':'setListener','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'isLimitWithTime','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_lockTotal','type':'uint256'}],'name':'setLockTotal','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_issuanceFinished','type':'bool'}],'name':'setAllowIssuance','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_value','type':'uint256'}],'name':'convert_From_DN_Dollar_To_DN','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'limitTimeTransferEndDate','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_limitedWalletsManager','type':'address'}],'name':'changeLimitedWalletsManager','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'LOCK_WITH_FOUR_WEEKS','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'setLockEightWeeks','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'unlockTotal','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'eventListener','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'newOwner','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'confirmOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_spender','type':'address'},{'name':'_addedValue','type':'uint256'}],'name':'increaseApproval','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_value','type':'uint256'}],'name':'convert_From_DN_To_DN_Dollar','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'isLimitEnabled','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_spender','type':'address'}],'name':'allowance','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_allowTransfers','type':'bool'}],'name':'setAllowTransfers','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'isAllowConvertExchange','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transferFromByOwner','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'balanceDollars','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'setLimitInAYear','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transferDollarFromByOwner','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_wallet','type':'address'}],'name':'addLimitedWalletAddress','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_wallet','type':'address'},{'name':'_value','type':'uint256'}],'name':'convert_From_DN_To_DN_Dollar_By_Owner','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_wallet','type':'address'},{'name':'_value','type':'uint256'}],'name':'convert_From_DN_Dollar_To_DN_By_Owner','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'inputs':[],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_isLock','type':'bool'}],'name':'AllowLockTokenForTime','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_address','type':'address'},{'indexed':false,'name':'_value','type':'uint256'},{'indexed':false,'name':'_receive','type':'uint256'}],'name':'ConvertDN_DNDollar','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_address','type':'address'},{'indexed':false,'name':'_value','type':'uint256'},{'indexed':false,'name':'_receive','type':'uint256'}],'name':'ConvertDNDollar_DN','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'isAllowConvertExchange','type':'bool'}],'name':'AllowConvertExchange','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_value','type':'uint256'}],'name':'ConfigRateExchange','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_isAllowTransferDollar','type':'bool'}],'name':'AllowTransferDollar','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_from','type':'address'},{'indexed':false,'name':'_to','type':'address'},{'indexed':false,'name':'_value','type':'uint256'}],'name':'TransferDollar','type':'event'},{'anonymous':false,'inputs':[],'name':'TransfersEnabled','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_newState','type':'bool'}],'name':'AllowTransfersChanged','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_to','type':'address'},{'indexed':false,'name':'_value','type':'uint256'}],'name':'Issue','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_from','type':'address'},{'indexed':false,'name':'_value','type':'uint256'}],'name':'Destroy','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'_issuanceFinished','type':'bool'}],'name':'IssuanceFinished','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'previousOwner','type':'address'},{'indexed':false,'name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_from','type':'address'},{'indexed':true,'name':'_to','type':'address'},{'indexed':false,'name':'_value','type':'uint256'}],'name':'Transfer','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_spender','type':'address'},{'indexed':false,'name':'_value','type':'uint256'}],'name':'Approval','type':'event'}]";
                var url = AppSettingHelper.GetEthereumNode();
                string contractName = "King.sol";
                int accountUnlockTime = 120;
                string accountPublicKey = "0xC2A213f481b2C794704218365E13d4761820B398";
                string accountPassword = "08786bec5c4b2ac8cb6dcafb320a3486d59a0e9f81860792fe13ca0a962cda3e";
                var contractAddress = "0x970de7a6ab0e062c5cfae6d342887b5eac1a3552";
                HexBigInteger gas = new HexBigInteger(3000000);
                var multiplier = 7;


              //  bool unlockResult = await Web3Api.UnlockAccount(accountPublicKey, accountPassword, (ulong)accountUnlockTime);
                Nethereum.Contracts.Contract contract = Web3Api.GetContract(abi, contractAddress);
                var funct = Web3Api.getFunction(contract, "balances");
                var result = await funct.CallAsync<System.Numerics.BigInteger>("0xc942F1D286d9b8002206CbB3196f46Fa892aAD93");
                Console.WriteLine("Result of function getOwner", result);



                return null;
           
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

    public class Web3Api
    {
        //Singleton
        private static Web3 _web3;
        public static Web3 mWeb3
        {
            get
            {
                if (_web3 == null)
                {
                    _web3 = new Nethereum.Web3.Web3(AppSettingHelper.GetEthereumNode());
                }

                return _web3;
            }
        }

        public static async Task<bool> UnlockAccount(string accountPublicKey, string accountPassword, ulong accountUnlockTime)
        {
            bool unlockResult = await mWeb3.Personal.UnlockAccount.SendRequestAsync(accountPublicKey, accountPassword, accountUnlockTime);

            return unlockResult;
        }

        //public static async Task<TheContract> GetTheContract(string contractName)
        //{
        //    string actionUrl = Utility.CombineUri(Link.HOST, "smart-contract/get-the-contract");
        //    string pathFile = Utility.GetPathFileContract(contractName);

        //    if (!System.IO.File.Exists(pathFile))
        //    {
        //        SanitaLog.Error("Not find " + pathFile);
        //        return null;
        //    }
        //    HttpContent fileStreamContent = new ByteArrayContent(System.IO.File.ReadAllBytes(pathFile));

        //    using (HttpClient client = new HttpClient())
        //    using (MultipartFormDataContent formData = new MultipartFormDataContent())
        //    {
        //        formData.Add(fileStreamContent, "file", contractName);

        //        var response = await client.PostAsync(actionUrl, formData);

        //        response.EnsureSuccessStatusCode();

        //        var contentString = await response.Content.ReadAsStringAsync();
        //        var contents = JObject.Parse(contentString);
        //        ResultApi result = JsonConvert.DeserializeObject<ResultApi>(contentString);

        //        client.Dispose();

        //        if (result.status == "success")
        //        {
        //            TheContract mTheContract = JsonConvert.DeserializeObject<TheContract>(result.data);
        //            SanitaLog.Log("Abi", mTheContract.Abi);
        //            SanitaLog.Log("ByteCode", mTheContract.Bytecode);
        //            return mTheContract;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //Deploy the contract
        public static async Task<string> WaitDeploy(string abi, string byteCode, string accountPublicKey, HexBigInteger gas, params object[] values)
        {
            string transactionHash = await Web3Api.mWeb3.Eth.DeployContract.SendRequestAsync(abi, byteCode, accountPublicKey, gas, values);
            return transactionHash;
        }

        //Mine the transaction of deployment and Get a receipt for that transaction
        public static async Task<TransactionReceipt> WaitMiner(string transactionHash)
        {
            TransactionReceipt receipt = null;

            // If receipt is null it means the Contract creation transaction is not minded yet.
            while (receipt == null)
            {
                receipt = await mWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                Thread.Sleep(4000);
            }



            //string contractAddress = receipt.ContractAddress;
            //var EthGetTransactionReceipt = Web3Api.mWeb3.Eth.Transactions.GetTransactionReceipt;
            //var EthCall = Web3Api.mWeb3.Eth.Transactions.Call;
            //var EthEstimateGas = Web3Api.mWeb3.Eth.Transactions.EstimateGas;
            //var EthGetTransactionByBlockHashAndIndex = Web3Api.mWeb3.Eth.Transactions.GetTransactionByBlockHashAndIndex;
            //var NetPeerCount = Web3Api.mWeb3.Net.PeerCount;
            //var EthGetBalance = Web3Api.mWeb3.Eth.GetBalance;
            //var EthMining = Web3Api.mWeb3.Eth.Mining.IsMining;
            //var EthAccounts = Web3Api.mWeb3.Eth.Accounts;

            return receipt;
        }

        #region CALL Multiply

        // Retrieve the total number of transactions of your sender address

        public static Nethereum.Contracts.Contract GetContract(string abi, string contractAddress)
        {
            Nethereum.Contracts.Contract contract = mWeb3.Eth.GetContract(abi, contractAddress);
            return contract;
        }

        public static async Task<HexBigInteger> GetTotalTransactions(string senderAddress)
        {
            HexBigInteger transactionCount = await Web3Api.mWeb3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderAddress);
            return transactionCount;
        }

        public static Function getFunction(Nethereum.Contracts.Contract contract, string functionName)
        {
            Function funct = contract.GetFunction(functionName);
            return funct;
        }

        #endregion CALL Multiply
    }

}