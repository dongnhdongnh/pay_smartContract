using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Helpers;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities.BTC
{
    public class BitcoinTransaction : BlockchainTransaction
    {
        public string BlockHash { get; set; }
    }

    [Table("BitcoinDepositTransaction")]
    public class BitcoinDepositTransaction : BitcoinTransaction
    {
        public static BitcoinDepositTransaction FromJson(string json) =>
            JsonHelper.DeserializeObject<BitcoinDepositTransaction>(json, JsonHelper.CONVERT_SETTINGS);

    }

    [Table("BitcoinWithdrawTransaction")]
    public class BitcoinWithdrawTransaction : BitcoinTransaction
    {
    }
}