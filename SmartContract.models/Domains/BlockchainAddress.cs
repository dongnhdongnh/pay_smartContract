using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Constants;
using SmartContract.models.Entities.BTC;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Entities.VAKA;

namespace SmartContract.models.Domains
{
    public abstract class BlockchainAddress : BaseModel
    {
        public string Address { get; set; }
        public string Label { get; set; }
        public string WalletId { get; set; }
        public string Status { get; set; }

        [NotMapped] //database attribute define
        public string Network
        {
            get
            {
                switch (GetType().Name)
                {
                    case nameof(BitcoinAddress):
                        return CryptoCurrency.BTC;
                    case nameof(EthereumAddress):
                        return CryptoCurrency.ETH;
                    case nameof(VakacoinAccount):
                        return CryptoCurrency.VAKA;
                    default:
                        throw new Exception("Network not defined!");
                }
            }
        }

        public abstract string GetSecret();
    }
}