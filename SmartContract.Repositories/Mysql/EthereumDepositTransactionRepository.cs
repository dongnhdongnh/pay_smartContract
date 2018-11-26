using System.Data;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql.Base;

namespace SmartContract.Repositories.Mysql
{
   public class EthereumDepositTransactionRepository : BlockchainTransactionRepository<EthereumTransaction.EthereumDepositTransaction>,
        IEthereumDepositTransactionRepository
    {
        public EthereumDepositTransactionRepository(string connectionString) : base(connectionString)
        {
        }

        public EthereumDepositTransactionRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }


    }
}