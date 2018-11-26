using System.Collections.Generic;

namespace SmartContract.models.Repositories.Base
{
    public interface IRepositoryBlockchainTransaction<TBlockchainTransaction> :
        IRepositoryTransaction<TBlockchainTransaction>,
        IRepositoryBase<TBlockchainTransaction>
    {
        List<TBlockchainTransaction> FindTransactionsNotCompleteOnNet();
    }
}