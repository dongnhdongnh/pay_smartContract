using SmartContract.Commons.Constants;
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
      //      var walletBusiness = new WalletBusiness.WalletBusiness(repoFactory);
            var connection = repoFactory.GetOldConnection() ?? repoFactory.GetDbConnection();
            try
            {
                while (true)
                {
                    Console.WriteLine("==========Start Scan Ethereum==========");

                    var rpc = new EthereumRpc(AppSettingHelper.GetEthereumNode());

                    using (var ethereumRepo = repoFactory.GetEthereumWithdrawTransactionRepository(connection))
                    {
                        using (var ethereumDepoRepo = repoFactory.GetEthereumDepositeTransactionRepository(connection))
                        {
                            var resultSend =
                                ethereumBusiness
                                    .ScanBlockAsync<models.Entities.ETH.EthereumTransaction.EthereumWithdrawTransaction, models.Entities.ETH.EthereumTransaction.EthereumDepositTransaction,
                                        models.Entities.ETH.EthereumBlockResponse, EthereumTransactionResponse>(CryptoCurrency.ETH, walletBusiness,
                                        ethereumRepo, ethereumDepoRepo, rpc);
                            Console.WriteLine(JsonHelper.SerializeObject(resultSend.Result));


                            Console.WriteLine("==========Scan Ethereum End==========");
                            Console.WriteLine("==========Wait for next scan==========");
                            Thread.Sleep(5000);
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
