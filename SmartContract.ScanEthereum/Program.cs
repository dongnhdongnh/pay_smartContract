﻿using SmartContract.Commons.Constants;
using SmartContract.Commons.Helpers;
using SmartContract.EthereumBusiness;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartContract.ScanEthereum
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var repositoryConfig = new RepositoryConfiguration
            {
                ConnectionString = AppSettingHelper.GetDbConnection()
            };

            RunScan(repositoryConfig);
        }


        private static void RunScan(RepositoryConfiguration repositoryConfig)
        {
            var repoFactory = new SmartContractRepositoryMysqlPersistenceFactory(repositoryConfig);

            var ethereumBusiness = new EthereumBusiness.EthereumBusiness(repoFactory);
            var rpc = new EthereumRpc(AppSettingHelper.GetEthereumNode());
            rpc.GetBlockNumber();
            var result = rpc.GetBlockNumber();
            if (result.Status == Status.STATUS_ERROR)
            {
                throw new Exception("Cant GetBlockNumber");
            }
            int blockNumber = 0;
            if (!int.TryParse(result.Data, out blockNumber))
            {
                throw new Exception("Cant parse block number");
            }
            CacheHelper.SetCacheString(String.Format(RedisCacheKey.KEY_SCANBLOCK_LASTSCANBLOCK,
                       CryptoCurrency.ETH), blockNumber.ToString());
            //      var walletBusiness = new WalletBusiness.WalletBusiness(repoFactory);
            var connection = repoFactory.GetOldConnection() ?? repoFactory.GetDbConnection();
            try
            {
                while (true)
                {
                    Console.WriteLine("==========Start Scan Ethereum==========");



                    // var rpc = new EthereumRpc(AppSettingHelper.GetEthereumNode());
                    //  var _re = rpc.FindTransactionByHash("0xe62ea756f9dbb5eb2439ea946eab1a3413c5517ebe6eecfb6d067a5d849fcf1f");
                    // foreach(var tran in _re.)
                    //var _input = rpc.DecodeInput("0xa9059cbb0000000000000000000000003a2e25cfb83d633c184f6e4de1066552c5bf45170000000000000000000000000000000000000000000000008ac7230489e80000");
                    using (var ethereumRepo = repoFactory.GetEthereumWithdrawTransactionRepository(connection))
                    {
                        using (var ethereumDepoRepo = repoFactory.GetEthereumDepositeTransactionRepository(connection))
                        {
                            var resultSend =
                                ethereumBusiness
                                    .ScanBlockAsync<models.Entities.ETH.EthereumTransaction.EthereumWithdrawTransaction, models.Entities.ETH.EthereumTransaction.EthereumDepositTransaction,
                                        models.Entities.ETH.EthereumBlockResponse, EthereumTransactionResponse>(CryptoCurrency.ETH,
                                        ethereumRepo, ethereumDepoRepo, rpc);
                            Console.WriteLine(JsonHelper.SerializeObject(resultSend.Result));


                            Console.WriteLine("==========Scan Ethereum End==========");
                            Console.WriteLine("==========Wait for next scan==========");
                            Thread.Sleep(50000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                connection.Close();
                Console.WriteLine(e.ToString());
            }
        }
    }
}
