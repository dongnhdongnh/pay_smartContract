using System;
using System.Threading;
using SmartContract.Commons.Helpers;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql;

namespace SmartContract.CreateAddress
{
    class Program
    {
        static void Main(string[] args)
        {
            var persistenceFactory = new SmartContractRepositoryMysqlPersistenceFactory(new RepositoryConfiguration
            {
                ConnectionString = AppSettingHelper.GetDbConnection()
            });


            var addAddressBusiness = new AutoCreateAddress.AutoCreateAddress(persistenceFactory);
            
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