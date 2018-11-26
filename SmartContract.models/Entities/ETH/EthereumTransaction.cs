using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities.ETH
{
    public class EthereumTransaction : BlockchainTransaction
    {
        [Table("EthereumDepositTransaction")]
        public class EthereumDepositTransaction : EthereumTransaction
        {
        }

        [Table("EthereumWithdrawTransaction")]
        public class EthereumWithdrawTransaction : EthereumTransaction
        {
        }
    }
}