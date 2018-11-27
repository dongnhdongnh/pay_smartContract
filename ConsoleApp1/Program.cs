using SmartContract.EthereumBusiness;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        // private EthereumRpc ethe;
        static void Main(string[] args)
        {
            try
            {
                EthereumRpc ethe = new EthereumRpc("");
                ethe.TestSmartContractFunction().Wait();
                // var T = await ethe.SendTransactionAsync1(null);
                //T.Wait();
                // await Task.Delay(1000);
                Console.WriteLine("Hello World!");
                Console.Read();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }


    }
}
