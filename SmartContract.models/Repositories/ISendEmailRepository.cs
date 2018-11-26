using System;
using SmartContract.models.Entities;
using SmartContract.models.Repositories.Base;

namespace SmartContract.models.Repositories
{
    public interface ISendEmailRepository : IRepositoryBase<EmailQueue>, IMultiThreadUpdateEntityRepository<EmailQueue>,IDisposable
    {

    }
}