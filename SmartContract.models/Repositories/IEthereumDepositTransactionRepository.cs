using System;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Repositories.Base;

namespace SmartContract.models.Repositories
{
    public interface
        IEthereumDepositTransactionRepository : IRepositoryBlockchainTransaction<EthereumTransaction.EthereumDepositTransaction>,IDisposable
    {
    }
}