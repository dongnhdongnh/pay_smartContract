using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using SmartContract.Commons.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartContract.EthereumBusiness
{
    public class Web3Api
    {

        //Singleton
        private static Web3 _web3;
        public static Web3 mWeb3
        {
            get
            {
                if (_web3 == null)
                {
                    var account = new Account(AppSettingHelper.GetSmartContractPrivateKey());
                    _web3 = new Nethereum.Web3.Web3(account, AppSettingHelper.GetEthereumNode());
                    _web3.TransactionManager.DefaultGas = 40000;
                    //  _web3.TransactionManager.DefaultGasPrice = Transaction.DEFAULT_GAS_PRICE;
                }

                return _web3;
            }
        }
        public static void DecodeInput(string input, Nethereum.Contracts.Contract contracts)
        {
            //mWeb3.Eth.
            //Nethereum.ABI.FunctionEncoding.ParameterDecoder dec = new Nethereum.ABI.FunctionEncoding.ParameterDecoder();
            //dec.DecodeDefaultData(input,);
       //   = new Nethereum.Contracts.Contract();


        }
        public static async Task<bool> UnlockAccount(string accountPublicKey, string accountPassword, ulong accountUnlockTime)
        {
            bool unlockResult = await mWeb3.Personal.UnlockAccount.SendRequestAsync(accountPublicKey, accountPassword, accountUnlockTime);

            return unlockResult;
        }

        //public static async Task<TheContract> GetTheContract(string contractName)
        //{
        //    string actionUrl = Utility.CombineUri(Link.HOST, "smart-contract/get-the-contract");
        //    string pathFile = Utility.GetPathFileContract(contractName);

        //    if (!System.IO.File.Exists(pathFile))
        //    {
        //        SanitaLog.Error("Not find " + pathFile);
        //        return null;
        //    }
        //    HttpContent fileStreamContent = new ByteArrayContent(System.IO.File.ReadAllBytes(pathFile));

        //    using (HttpClient client = new HttpClient())
        //    using (MultipartFormDataContent formData = new MultipartFormDataContent())
        //    {
        //        formData.Add(fileStreamContent, "file", contractName);

        //        var response = await client.PostAsync(actionUrl, formData);

        //        response.EnsureSuccessStatusCode();

        //        var contentString = await response.Content.ReadAsStringAsync();
        //        var contents = JObject.Parse(contentString);
        //        ResultApi result = JsonConvert.DeserializeObject<ResultApi>(contentString);

        //        client.Dispose();

        //        if (result.status == "success")
        //        {
        //            TheContract mTheContract = JsonConvert.DeserializeObject<TheContract>(result.data);
        //            return mTheContract;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //Deploy the contract
        public static async Task<string> WaitDeploy(string abi, string byteCode, string accountPublicKey, HexBigInteger gas, params object[] values)
        {
            string transactionHash = await Web3Api.mWeb3.Eth.DeployContract.SendRequestAsync(abi, byteCode, accountPublicKey, gas, values);
            return transactionHash;
        }

        //Mine the transaction of deployment and Get a receipt for that transaction
        public static async Task<TransactionReceipt> WaitMiner(string transactionHash)
        {
            TransactionReceipt receipt = null;

            // If receipt is null it means the Contract creation transaction is not minded yet.
            while (receipt == null)
            {
                receipt = await mWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

                Thread.Sleep(4000);
            }



            //string contractAddress = receipt.ContractAddress;
            //var EthGetTransactionReceipt = Web3Api.mWeb3.Eth.Transactions.GetTransactionReceipt;
            //var EthCall = Web3Api.mWeb3.Eth.Transactions.Call;
            //var EthEstimateGas = Web3Api.mWeb3.Eth.Transactions.EstimateGas;
            //var EthGetTransactionByBlockHashAndIndex = Web3Api.mWeb3.Eth.Transactions.GetTransactionByBlockHashAndIndex;
            //var NetPeerCount = Web3Api.mWeb3.Net.PeerCount;
            //var EthGetBalance = Web3Api.mWeb3.Eth.GetBalance;
            //var EthMining = Web3Api.mWeb3.Eth.Mining.IsMining;
            //var EthAccounts = Web3Api.mWeb3.Eth.Accounts;

            return receipt;
        }

        #region CALL Multiply

        // Retrieve the total number of transactions of your sender address

        public static Nethereum.Contracts.Contract GetContract(string abi, string contractAddress)
        {
            Nethereum.Contracts.Contract contract = mWeb3.Eth.GetContract(abi, contractAddress);
            return contract;
        }

        public static async Task<HexBigInteger> GetTotalTransactions(string senderAddress)
        {
            HexBigInteger transactionCount = await Web3Api.mWeb3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderAddress);
            return transactionCount;
        }

        public static Function getFunction(Nethereum.Contracts.Contract contract, string functionName)
        {
            Function funct = contract.GetFunction(functionName);
            return funct;
        }

        public static List<ParameterOutput> DecodeInput(string input, Function funtion)
        {
            return funtion.DecodeInput(input);
            //mWeb3.Eth.
            //Nethereum.ABI.FunctionEncoding.ParameterDecoder dec = new Nethereum.ABI.FunctionEncoding.ParameterDecoder();
            //dec.DecodeDefaultData(input,);
            //   = new Nethereum.Contracts.Contract();
            // contracts.

        }
        #endregion CALL Multiply
    }
}
