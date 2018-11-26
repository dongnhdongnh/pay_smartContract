using System.ComponentModel.DataAnnotations.Schema;
using SmartContract.Commons.Constants;
using SmartContract.models.Domains;

namespace SmartContract.models.Entities.VAKA
{
    [Table("VakacoinTransaction")]
    public class VakacoinTransaction : BlockchainTransaction
    {
        public string Memo { get; set; }

        public string GetStringAmount()
        {
            return CryptoCurrency.GetAmount(CryptoCurrency.VAKA, Amount);
        }
    }

    [Table("VakacoinDepositTransaction")]
    public class VakacoinDepositTransaction : VakacoinTransaction
    {
        public string TrxId { get; set; }
    }

    [Table("VakacoinWithdrawTransaction")]
    public class VakacoinWithdrawTransaction : VakacoinTransaction
    {
    }
}