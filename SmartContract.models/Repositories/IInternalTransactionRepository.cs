using System;
using SmartContract.models.Entities;
using SmartContract.models.Repositories.Base;

namespace SmartContract.models.Repositories
{
    public interface IInternalTransactionRepository : IRepositoryBlockchainTransaction<InternalWithdrawTransaction>, IDisposable
    {
    }
}