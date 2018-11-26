using SmartContract.models.Domains;
using System.Threading.Tasks;

namespace SmartContract.BlockchainBusiness
{
    public interface IBlockchainRpc
    {
        string EndPointUrl { get; set; }

        /// <summary>
        /// create new address with password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        ReturnObject CreateNewAddress(string password);

        /// <summary>
        /// Create new address without params
        /// </summary>
        /// <returns></returns>
        ReturnObject CreateNewAddress();

        /// <summary>
        /// Create address with private key and public key
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        ReturnObject CreateNewAddress(string privateKey, string publicKey);

        /// <summary>
        /// Send transaction with raw data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ReturnObject SendRawTransaction(string data);

        /// <summary>
        /// Get Balance of address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ReturnObject GetBalance(string address);

        /// <summary>
        /// Sign transaction with private Key
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="transactionData"></param>
        /// <returns></returns>
        ReturnObject SignTransaction(string privateKey, object[] transactionData);

        /// <summary>
        /// Send asyn transaction with transaction data
        /// </summary>
        /// <param name="blockchainTransaction"></param>
        /// <returns></returns>
        Task<ReturnObject> SendTransactionAsync(BlockchainTransaction blockchainTransaction);

        /// <summary>
        /// Get block by block number
        /// </summary>
        /// <param name="blockNumber"></param>
        /// <returns></returns>
        ReturnObject GetBlockByNumber(int blockNumber);

        /// <summary>
        /// Get number of blocks
        /// </summary>
        /// <returns></returns>
        ReturnObject GetBlockNumber();

        /// <summary>
        /// Get Transaction by hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        ReturnObject FindTransactionByHash(string hash);
    }
}