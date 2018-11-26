using System.Data;
using MySql.Data.MySqlClient;
using SmartContract.models.Repositories;

namespace SmartContract.Repositories.Mysql
{
    public class SmartContractRepositoryMysqlPersistenceFactory : ISmartContractRepositoryFactory
    {
        public RepositoryConfiguration RepositoryConfiguration { get; }

        public IDbConnection Connection { get; set; }

        public SmartContractRepositoryMysqlPersistenceFactory(RepositoryConfiguration repositoryConfiguration)
        {
            RepositoryConfiguration = repositoryConfiguration;
        }

        public IDbConnection GetDbConnection()
        {
            Connection = new MySqlConnection(RepositoryConfiguration.ConnectionString);
            return Connection;
        }

        public IDbConnection GetOldConnection()
        {
            return Connection ?? (Connection = new MySqlConnection(RepositoryConfiguration.ConnectionString));
        }


        public IUserRepository GetUserRepository(IDbConnection dbConnection)
        {
            return new UserRepository(dbConnection);
        }


        public IEthereumWithdrawTransactionRepository GetEthereumWithdrawTransactionRepository(
            IDbConnection dbConnection)
        {
            return new EthereumWithdrawnTransactionRepository(dbConnection);
        }

        public IEthereumDepositTransactionRepository GetEthereumDepositeTransactionRepository(
            IDbConnection dbConnection)
        {
            return new EthereumDepositTransactionRepository(dbConnection);
        }


        public ISendEmailRepository GetSendEmailRepository(IDbConnection dbConnection)
        {
            return new SendEmailRepository(dbConnection);
        }
        
        public IInternalTransactionRepository GetInternalTransactionRepository(IDbConnection dbConnection)
        {
            return new InternalTransactionsRepository(dbConnection);
        }
    }
}