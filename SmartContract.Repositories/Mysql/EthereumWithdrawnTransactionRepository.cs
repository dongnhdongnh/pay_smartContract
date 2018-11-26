using System.Data;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Repositories;
using SmartContract.Repositories.Mysql.Base;

namespace SmartContract.Repositories.Mysql
{
    public class EthereumWithdrawnTransactionRepository : BlockchainTransactionRepository<EthereumTransaction.EthereumWithdrawTransaction>,
        IEthereumWithdrawTransactionRepository
    {
        public EthereumWithdrawnTransactionRepository(string connectionString) : base(connectionString)
        {
        }

        public EthereumWithdrawnTransactionRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }

    }
}