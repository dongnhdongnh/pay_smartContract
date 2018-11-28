using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using SmartContract.BlockchainBusiness;
using SmartContract.BlockchainBusiness.Base;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Repositories;
using SmartContract.models.Repositories.Base;

namespace SmartContract.EthereumBusiness
{
    public class EthereumBusiness : AbsBlockchainBusiness
    {
        public EthereumBusiness(ISmartContractRepositoryFactory smartContractRepositoryFactory, bool isNewConnection = true) : base(
            smartContractRepositoryFactory, isNewConnection)
        {
        }

        public ReturnObject FakePendingTransaction(EthereumTransaction.EthereumWithdrawTransaction blockchainTransaction)
        {
            try
            {
                using (var ethereumwithdrawRepo =
                    SmartContractRepositoryFactory.GetEthereumWithdrawTransactionRepository(DbConnection))
                {
                    blockchainTransaction.Status = Status.STATUS_PENDING;
                    return ethereumwithdrawRepo.Insert(blockchainTransaction);
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

        public ReturnObject FakeDepositTransaction(EthereumTransaction.EthereumDepositTransaction blockchainTransaction)
        {
            try
            {
                using (var repo = SmartContractRepositoryFactory.GetEthereumDepositeTransactionRepository(DbConnection))
                {
                    blockchainTransaction.Status = Status.STATUS_COMPLETED;
                    return repo.Insert(blockchainTransaction);
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

        public override List<BlockchainTransaction> GetWithdrawHistory(int offset = -1, int limit = -1,
            string[] orderBy = null)
        {
            using (var ethereumwithdrawRepo = SmartContractRepositoryFactory.GetEthereumWithdrawTransactionRepository(DbConnection))
            {
                Console.WriteLine("Get ETH HISTORY");
                return GetHistory<EthereumTransaction.EthereumWithdrawTransaction>(ethereumwithdrawRepo, offset, limit, orderBy);
            }
        }

        public override List<BlockchainTransaction> GetDepositHistory(int offset = -1, int limit = -1,
            string[] orderBy = null)
        {
            using (var ethereumDepositRepo = SmartContractRepositoryFactory.GetEthereumDepositeTransactionRepository(DbConnection))
            {
                return GetHistory<EthereumTransaction.EthereumDepositTransaction>(ethereumDepositRepo, offset, limit, orderBy);
            }
        }


        public override List<BlockchainTransaction> GetAllHistory(out int numberData, string userID, string currency,
            int offset = -1,
            int limit = -1, string[] orderBy = null, string search = null, long day = -1)
        {
            using (var depositRepo = SmartContractRepositoryFactory.GetEthereumDepositeTransactionRepository(DbConnection))
            {
                using (var withdrawRepo = SmartContractRepositoryFactory.GetEthereumWithdrawTransactionRepository(DbConnection))
                {
                    using (var inter = SmartContractRepositoryFactory.GetInternalTransactionRepository(DbConnection))
                    {
                        return GetAllHistory<EthereumTransaction.EthereumWithdrawTransaction, EthereumTransaction.EthereumDepositTransaction>(out numberData, userID,
                            currency, withdrawRepo, depositRepo, inter.GetTableName(), offset, limit, orderBy, search);

                    }
                }
            }
        }

        /// <summary>
        /// Created Address with optimistic lock
        /// </summary>
        /// <param name="rpcClass"></param>
        /// <param name="walletId"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ReturnObject CreateAddress(IBlockchainRpc rpcClass, string userId,
            string other = "")
        {
            try
            {
                using (var dbConnection = SmartContractRepositoryFactory.GetDbConnection())
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }

                    var userRepository = SmartContractRepositoryFactory.GetUserRepository(dbConnection);

                    var userCheck = userRepository.FindByIdUser(userId);

                    if (userCheck == null)
                        return new ReturnObject
                        {
                            Status = Status.STATUS_ERROR,
                            Message = "User Not Found"
                        };

                    var resultsRPC = rpcClass.CreateNewAddress(other);
                    if (resultsRPC.Status == Status.STATUS_ERROR)
                        return resultsRPC;

                    var address = resultsRPC.Data;

                    return new ReturnObject
                    {
                        Status = Status.STATUS_SUCCESS,
                        Data = address,
                        Message = "Can't update wallet " + userCheck.mem_id
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

        public virtual async Task<ReturnObject> ScanBlockAsync<TWithDraw, TDeposit, TBlockResponse, TTransaction>(
            string networkName,
            IRepositoryBlockchainTransaction<TWithDraw> withdrawRepoQuery,
            IRepositoryBlockchainTransaction<TDeposit> depositRepoQuery,
            EthereumRpc rpcClass)
            where TWithDraw : BlockchainTransaction
            where TDeposit : BlockchainTransaction
            where TBlockResponse : EthereumBlockResponse
            where TTransaction : EthereumTransactionResponse
        {
            try
            {

                int lastBlock = -1;
                int blockNumber = -1;
                //Get lastBlock from last time
                int.TryParse(
                    CacheHelper.GetCacheString(String.Format(RedisCacheKey.KEY_SCANBLOCK_LASTSCANBLOCK,
                        networkName)), out lastBlock);
                if (lastBlock < 0)
                    lastBlock = 0;

                //get blockNumber:
                var result = rpcClass.GetBlockNumber();
                if (result.Status == Status.STATUS_ERROR)
                {
                    throw new Exception("Cant GetBlockNumber");
                }

                if (!int.TryParse(result.Data, out blockNumber))
                {
                    throw new Exception("Cant parse block number");
                }

                //Get list of new block that have transactions
                if (lastBlock >= blockNumber)
                    lastBlock = blockNumber;
                //lastBlock = 4519651;
                //blockNumber = 4519660;
                Console.WriteLine("SCAN FROM " + lastBlock + "___" + blockNumber);
                List<TBlockResponse> blocks = new List<TBlockResponse>();
                for (int i = lastBlock; i <= blockNumber - 1; i++)
                {
                    if (i < 0) continue;
                    result = rpcClass.GetBlockByNumber(i);
                    if (result.Status == Status.STATUS_ERROR)
                    {
                        return result;
                    }

                    if (result.Data == null)
                        continue;
                    TBlockResponse block = JsonHelper.DeserializeObject<TBlockResponse>(result.Data);
                    if (block.TransactionsResponse.Length > 0)
                    {
                        //if (block.T.Equals(AppSettingHelper.GetSmartContractAddress()))
                        //{
                        //    Console.WriteLine(JsonHelper.SerializeObject(block));
                        //}
                        //else
                        //{

                        //}

                        blocks.Add(block);
                        //foreach (var trans in block.TransactionsResponse)
                        //{
                        //    if (trans.To != null)
                        //        if (AppSettingHelper.GetSmartContractAddress().ToLower().Equals(trans.To.ToLower()))
                        //        {
                        //            var paras = rpcClass.DecodeInput(trans.Input);
                        //            foreach (var para in paras)
                        //            {
                        //                Console.WriteLine(para.Parameter.Name + "_" + para.Result.ToString());
                        //            }
                        //            //  Console.WriteLine(JsonHelper.SerializeObject(trans));
                        //        }
                        //}
                        Console.WriteLine("Add block " + i);
                    }
                }
                Console.WriteLine("Total block has transaction: " + blocks.Count);
                CacheHelper.SetCacheString(String.Format(RedisCacheKey.KEY_SCANBLOCK_LASTSCANBLOCK, networkName),
                    blockNumber.ToString());
                if (blocks.Count <= 0)
                {
                    throw new Exception("no blocks have transaction");
                }
                //Get done,List<> blocks now contains all block that have transaction
                //check Transaction and update:
                //Search transactions which need to scan:

                //var withdrawPendingTransactions = withdrawRepoQuery.FindTransactionsNotCompleteOnNet();
                ////Scan all block and check Withdraw transaction:
                //Console.WriteLine("Scan withdrawPendingTransactions");
                //if (withdrawPendingTransactions.Count > 0)
                //{
                //    foreach (TBlockResponse block in blocks)
                //    {
                //        if (withdrawPendingTransactions.Count <= 0)
                //        {
                //            //SCAN DONE:
                //            break;
                //        }

                //        for (int i = withdrawPendingTransactions.Count - 1; i >= 0; i--)
                //        {
                //            BlockchainTransaction currentPending = withdrawPendingTransactions[i];
                //            EthereumTransactionResponse trans =
                //                block.TransactionsResponse.SingleOrDefault(x => x.Hash.Equals(currentPending.Hash));
                //            int _blockNumber = -1;
                //            int fee = 0;

                //            if (trans != null)
                //            {
                //                trans.BlockNumber.HexToInt(out _blockNumber);
                //                if (trans.Fee != null)
                //                    trans.Fee.HexToInt(out fee);
                //                Console.WriteLine("HELLO " + currentPending.Hash);
                //                currentPending.BlockNumber = _blockNumber;
                //                currentPending.Fee = fee;
                //                currentPending.UpdatedAt = (int)CommonHelper.GetUnixTimestamp();
                //                //	_currentPending.Status = Status.StatusCompleted;
                //                //	_currentPending.InProcess = 0;
                //                Console.WriteLine("CaLL UPDATE");

                //                withdrawRepoQuery.Update((TWithDraw)currentPending);
                //                withdrawPendingTransactions.RemoveAt(i);
                //            }
                //        }
                //    }
                //}

                Console.WriteLine("Scan withdrawPendingTransactions Done");
                using (var dbConnection = SmartContractRepositoryFactory.GetDbConnection())
                {
                    var userRepository = SmartContractRepositoryFactory.GetUserRepository(dbConnection);


                    //check wallet balance and update 
                    foreach (TBlockResponse block in blocks)
                    {
                        foreach (EthereumTransactionResponse trans in block.TransactionsResponse)
                        {

                            if (trans.To != null)
                            {
                                if (AppSettingHelper.GetSmartContractAddress().ToLower().Equals(trans.To.ToLower()))
                                {
                                    var paras = rpcClass.DecodeInput(trans.Input);
                                    string _fromAddress = "";
                                    string _toAddress = "";
                                    string _value = "";

                                    foreach (var para in paras)
                                    {
                                        //Console.WriteLine(para.Parameter.Name + "_" + para.Result.ToString());
                                        switch (para.Parameter.Name)
                                        {
                                            case "_from":
                                                _fromAddress = para.Result.ToString();
                                                break;
                                            case "_to":
                                                _toAddress = para.Result.ToString();
                                                break;
                                            case "_value":
                                                _value = para.Result.ToString();
                                                break;
                                            default:
                                                break;
                                        }
                                    }


                                    var user = userRepository.FindByAddress(_toAddress);
                                    //  var user = userRepository.FindByAddress("123");
                                    //  var _sendWallet = wallet.FindWalletByAddressAndNetworkName(toAddress, networkName);
                                    if (user == null)
                                    //if(false)
                                    {
                                        //logger.Info(to + " is not exist in Wallet!!!");
                                        continue;
                                    }
                                    else
                                    {
                                        //Console.WriteLine("value" + _trans.value);
                                        //  if (trans.Value.HexToBigInteger(out var transactionValue))
                                        {
                                            // var userID = "";

                                            // if (_sendWallet != null)
                                            //wallet.UpdateBalanceDeposit(toAddress, EthereumRpc.WeiToEther(transactionValue),
                                            //  networkName);
                                            var _deposite = new EthereumTransaction.EthereumDepositTransaction();

                                            _deposite.Id = CommonHelper.GenerateUuid();
                                            _deposite.FromAddress = _fromAddress;
                                            _deposite.ToAddress = _toAddress;
                                            _deposite.UserId = user.mem_id;
                                            _deposite.Hash = trans.Hash;
                                           
                                            BigInteger amount = 0;
                                            if (BigInteger.TryParse(_value, out amount))
                                            {
                                                _deposite.Amount = EthereumRpc.WeiToEther(amount);
                                                userRepository.AddBalance(_deposite.UserId, _deposite.Amount);
                                            }
                                            int bNum = 0;
                                            if (trans.BlockNumber.HexToInt(out bNum))
                                                _deposite.BlockNumber = bNum;
                                            _deposite.Status = Status.STATUS_COMPLETED;
                                            _deposite.CreatedAt = (int)CommonHelper.GetUnixTimestamp();
                                            _deposite.UpdatedAt = (int)CommonHelper.GetUnixTimestamp();

                                            depositRepoQuery.Insert(_deposite as TDeposit);

                                        }
                                    }

                                    //  Console.WriteLine(JsonHelper.SerializeObject(trans));
                                }
                            }

                        }
                    }
                }


                return new ReturnObject
                {
                    Status = Status.STATUS_COMPLETED,
                    Message = "Scan done"
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
    }
}