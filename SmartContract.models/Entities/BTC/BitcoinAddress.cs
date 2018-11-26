using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities.BTC
{
    [Table("BitcoinAddress")]
    public class BitcoinAddress : BlockchainAddress
    {
        public override string GetSecret()
        {
            throw new System.NotImplementedException();
        }
    }
}