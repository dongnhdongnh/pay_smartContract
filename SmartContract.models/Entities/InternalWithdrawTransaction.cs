using SmartContract.models.Domains;

namespace SmartContract.models.Entities
{
    public class InternalWithdrawTransaction : MultiThreadUpdateModel
    {
        public string SenderUserId { get; set; }
        public string ReceiverUserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PricePerCoin { get; set; }
        public string Currency { get; set; }
        public string Idem { get; set; }

        public string Description { get; set; }
    }
}