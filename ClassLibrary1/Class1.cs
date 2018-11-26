using NUnit.Framework;
using SmartContract.EthereumBusiness;
using System;
using System.Threading.Tasks;

namespace ClassLibrary1
{

    class Class1
    {
        EthereumRpc _etheRpc;
        [SetUp]
        public void Setup()
        {
            _etheRpc = new EthereumRpc("http://localhost:9900");
        }
        [Test]
        public async Task SendTransactionAsync()
        {
            // var _etheRpc = new EthereumRpc("http://localhost:9900");

            var myResult = await _etheRpc.SendTransactionAsync1(null);
            Assert.IsNotNull(myResult);
            // var output = _etheRpc.SendTransactionWithPassphrase("0x12890d2cce102216644c59dae5baed380d84830c", "0x3a2e25cfb83d633c184f6e4de1066552c5bf4517", 10, "password");
            //var output = _etheRpc.SendTransaction("0x12890d2cce102216644c59dae5baed380d84830c", "0x3a2e25cfb83d633c184f6e4de1066552c5bf4517", 10);
            //Trace.Write(JsonHelper.SerializeObject(output));

            //  var _balance = _etheRpc.GetBalance("0x3a2e25cfb83d633c184f6e4de1066552c5bf4517");
            // Thread.Sleep(1000);
            //  var ba = 0;
            // _balance.Data.ToString().HexToInt(out ba);
            //  Console.WriteLine("BALANCE:" + ba);
            //Assert.IsNotNull(output);
        }
    }
}
