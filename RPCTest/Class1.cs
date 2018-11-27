using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RPCTest
{
    [TestFixture]
    public class Class1
    {
        EthereumRpc _rpc;
        [SetUp]
        public void SetUp()
        {
            this._rpc = new EthereumRpc();
        }

        [TestCase]
        public void Test1()
        { }

    }
}
