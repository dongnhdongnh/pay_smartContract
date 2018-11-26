﻿using SmartContract.Commons.Helpers;
using SmartContract.EthereumBusiness;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartContract.SendEthereum
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                CacheHelper.DeleteCacheString("cache");

                var repositoryConfig = new RepositoryConfiguration
                {
                    ConnectionString = AppSettingHelper.GetDbConnection()
                };
                string rootAddress = "0x12890d2cce102216644c59dae5baed380d84830c";
                string rootPassword = "password";
                EthereumRpc.SetAdminAddressPassword(rootAddress, rootPassword);
                for (var i = 0; i < 10; i++)
                {
                    var ts = new Thread(() => RunSend(repositoryConfig));
                    ts.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void RunSend(RepositoryConfiguration repositoryConfig)
        {
            var repoFactory = new SmartContractRepositoryMysqlPersistenceFactory(repositoryConfig);

            var ethereumBusiness = new EthereumBusiness.EthereumBusiness(repoFactory);
            var connection = repoFactory.GetDbConnection();
            try
            {
                while (true)
                {
                    Console.WriteLine("Start Send Ethereum....");

                    var rpc = new EthereumRpc(AppSettingHelper.GetEthereumNode());

                    using (var ethereumRepo = repoFactory.GetEthereumWithdrawTransactionRepository(connection))
                    {
                        var resultSend = ethereumBusiness.SendTransactionAsync(ethereumRepo, rpc);
                        Console.WriteLine(JsonHelper.SerializeObject(resultSend.Result));


                        Console.WriteLine("Send Ethereum End...");
                        Thread.Sleep(1000);
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