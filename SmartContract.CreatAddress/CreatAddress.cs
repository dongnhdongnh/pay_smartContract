using System;
using System.Data;
using System.Threading.Tasks;
using NLog;
using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.EthereumBusiness;
using SmartContract.models.Domains;
using SmartContract.models.Repositories;

namespace SmartContract.CreatAddress
{
    public class CreatAddress
    {
        private readonly ISmartContractRepositoryFactory _smartcontractRepositoryFactory;
        private readonly IDbConnection _connectionDb;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CreatAddress(ISmartContractRepositoryFactory smartcontractRepositoryFactory, bool isNewConnection = true)
        {
            _smartcontractRepositoryFactory = smartcontractRepositoryFactory;
            _connectionDb = isNewConnection
                ? smartcontractRepositoryFactory.GetDbConnection()
                : smartcontractRepositoryFactory.GetOldConnection();
        }

        /// <summary>
        /// Created Address with optimistic lock
        /// </summary>
        /// <param name="rpcClass"></param>
        /// <param name="walletId"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public async Task<ReturnObject> CreateAddressAsync()
        {
            try
            {
                using (var dbConnection = _smartcontractRepositoryFactory.GetDbConnection())
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }

                    var userRepository = _smartcontractRepositoryFactory.GetUserRepository(dbConnection);
                    var userPendding = userRepository.FindUserAddressNull();


                    if (userPendding?.Id == null)
                        return new ReturnObject
                        {
                            Status = Status.STATUS_ERROR,
                            Message = "User Not Found"
                        };


                    //begin first email
                    var transctionScope = dbConnection.BeginTransaction();
                    try
                    {
                        var lockResult = await userRepository.LockForProcess(userPendding);
                        if (lockResult.Status == Status.STATUS_ERROR)
                        {
                            transctionScope.Rollback();
                            return new ReturnObject
                            {
                                Status = Status.STATUS_SUCCESS,
                                Message = "Cannot Lock For Process"
                            };
                        }

                        transctionScope.Commit();
                    }
                    catch (Exception e)
                    {
                        transctionScope.Rollback();
                        return new ReturnObject
                        {
                            Status = Status.STATUS_ERROR,
                            Message = e.ToString()
                        };
                    }

                    //update Version to Model
                    userPendding.Version += 1;

                    var transactionSend = dbConnection.BeginTransaction();
                    try
                    {
                        var ethRpc = new EthereumRpc(AppSettingHelper.GetEthereumNode());
                        var pass = CommonHelper.RandomString(15);
                        var ethereumBusiness =
                            new EthereumBusiness.EthereumBusiness(_smartcontractRepositoryFactory);
                        var resultEthereum = ethereumBusiness.CreateAddress(ethRpc, userPendding.Id, pass);
                        if (resultEthereum.Status == Status.STATUS_ERROR)
                        {
                            transactionSend.Rollback();

                            return new ReturnObject
                            {
                                Status = Status.STATUS_ERROR,
                                Message = "Cannot create add bitcoin"
                            };
                        }

                        var address = resultEthereum.Data;

                        if (string.IsNullOrEmpty(address))
                        {
                            transactionSend.Rollback();

                            return new ReturnObject
                            {
                                Status = Status.STATUS_ERROR,
                                Message = "Cannot create address"
                            };
                        }

                        userPendding.IsProcessing = 0;
                        userPendding.Address = address;

                        var updateResult = userRepository.Update(userPendding);
                        if (updateResult.Status == Status.STATUS_ERROR)
                        {
                            transactionSend.Rollback();

                            return new ReturnObject
                            {
                                Status = Status.STATUS_ERROR,
                                Message = "Cannot update wallet status"
                            };
                        }

                        transactionSend.Commit();
                        return new ReturnObject
                        {
                            Status = Status.STATUS_SUCCESS,
                            Message = "Create success"
                        };
                    }
                    catch (Exception e)
                    {
                        // release lock
                        transactionSend.Rollback();
                        var releaseResult = await userRepository.ReleaseLock(userPendding);
                        Console.WriteLine(JsonHelper.SerializeObject(releaseResult));
                        throw;
                    }
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
    }
}