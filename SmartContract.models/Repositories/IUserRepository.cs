using System;
using System.Collections.Generic;
using SmartContract.models.Domains;
using SmartContract.models.Entities;
using SmartContract.models.Repositories.Base;

namespace SmartContract.models.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>, IDisposable
    {
        
        string QuerySearch(Dictionary<string, string> models);
        User FindWhere(string sql);
        string FindEmailBySendTransaction(BlockchainTransaction transaction);
        User FindByEmailAddress(string emailAddress);

        List<User> FindAllUser();
    }
}