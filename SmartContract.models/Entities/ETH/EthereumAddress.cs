using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities.ETH
{
    [Table("EthereumAddress")]
    public class EthereumAddress : BlockchainAddress
    {
        [JsonIgnore] public string Password { get; set; }

        public override string GetSecret()
        {
            return Password;
        }
    }
}