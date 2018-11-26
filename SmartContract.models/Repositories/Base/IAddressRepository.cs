using System.Collections.Generic;
using System.Threading.Tasks;
using SmartContract.models.Domains;

namespace SmartContract.models.Repositories.Base
{
    public interface IAddressRepository<TBlockchainAddress>
    {
        TBlockchainAddress FindByAddress(string address);
        List<TBlockchainAddress> FindByWalletId(string walletId);
        List<TBlockchainAddress> FindByUserIdAndCurrency(string user, string currency);
        Task<ReturnObject> InsertAddress(string address, string walletId, string other);
    }
}