using System.Collections.Generic;
using SmartContract.models.Domains;

namespace SmartContract.models.Repositories.Base
{
    public interface IRepositoryTransaction<TEntity> : IMultiThreadUpdateEntityRepository<TEntity>
    {

        List<BlockchainTransaction> FindTransactionHistory(int offset, int limit, string[] orderBy);

        List<BlockchainTransaction> FindTransactionHistoryAll(out int numberData, string userID, string currencyName,
            string tableNameWithdraw, string tableNameDeposit, string tableInternalWithdraw, int offset, int limit,
            string[] orderBy, string whereValue, long dayValue);

        string GetTableName();

        List<BlockchainTransaction> FindTransactionsByUserId(string userId);
        List<BlockchainTransaction> FindTransactionsFromAddress(string fromAddress);
        List<BlockchainTransaction> FindTransactionsToAddress(string toAddress);
        List<BlockchainTransaction> FindTransactionsInner(string innerAddress);
    }
}