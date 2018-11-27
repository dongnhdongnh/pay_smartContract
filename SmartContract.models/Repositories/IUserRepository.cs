using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        User FindByAddress(string address);
        User FindUserAddressNull();
        Task<ReturnObject> LockForProcessUser(User row);
        User FindByIdUser(string id);
        Task<ReturnObject> ReleaseLock(User row);
        ReturnObject UpdateUser(User objectUpdate);
        List<User> FindAllUser();
    }
}