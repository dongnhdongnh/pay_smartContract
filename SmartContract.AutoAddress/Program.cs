using System;
using System.Threading;
using SmartContract.Commons.Helpers;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql;

namespace SmartContract.AutoAddress
{
    class Program
    {
        static void Main(string[] args)
        {
            var persistenceFactory = new SmartContractRepositoryMysqlPersistenceFactory(new RepositoryConfiguration
            {
                ConnectionString = AppSettingHelper.GetDbConnection()
            });


            var addAddressBusiness = new CreatAddress.CreatAddress(persistenceFactory);

            while (true)
            {
                try
                {
                    var result = addAddressBusiness.CreateAddressAsync();
                    Console.WriteLine(JsonHelper.SerializeObject(result));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(1000);
            }
        }
    }
}