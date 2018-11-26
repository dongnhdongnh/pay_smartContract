using System.Data;

namespace SmartContract.models.Repositories
{
    public interface ISmartContractRepositoryFactory
    {
        IDbConnection GetDbConnection();
        IDbConnection GetOldConnection();
        IUserRepository GetUserRepository(IDbConnection dbConnection);
        IEthereumWithdrawTransactionRepository GetEthereumWithdrawTransactionRepository(IDbConnection dbConnection);
        IEthereumDepositTransactionRepository GetEthereumDepositeTransactionRepository(IDbConnection dbConnection);
        ISendEmailRepository GetSendEmailRepository(IDbConnection dbConnection);
        IInternalTransactionRepository GetInternalTransactionRepository(IDbConnection dbConnection);
    }
}