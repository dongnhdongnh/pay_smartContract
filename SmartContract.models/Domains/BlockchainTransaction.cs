using System;
using System.Linq;
using SmartContract.Commons.Constants;
using SmartContract.models.Entities.BTC;
using SmartContract.models.Entities.ETH;
using SmartContract.models.Entities.VAKA;

namespace SmartContract.models.Domains
{
    public class BlockchainTransaction : MultiThreadUpdateModel
    {
        public string UserId { get; set; }
        public string Hash { get; set; }
        public int BlockNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal PricePerCoin { get; set; }

        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public decimal Fee { get; set; }


        public string Description { get; set; }

        //[Write(false)]
        //[Computed]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int Type { get; set; }



        public string NetworkName()
        {
            switch (GetType().Name)
            {
                case nameof(BitcoinDepositTransaction):
                case nameof(BitcoinWithdrawTransaction):
                case nameof(BitcoinTransaction):
                    return CryptoCurrency.BTC;
                case nameof(EthereumTransaction.EthereumDepositTransaction):
                case nameof(EthereumTransaction.EthereumWithdrawTransaction):
                case nameof(EthereumTransaction):
                    return CryptoCurrency.ETH;
                case nameof(VakacoinDepositTransaction):
                case nameof(VakacoinWithdrawTransaction):
                case nameof(VakacoinTransaction):
                    return CryptoCurrency.VAKA;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Convert this BlockchainTransaction to deliver class by copy every readable and writable properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToDelivered<T>() where T : BlockchainTransaction, new()
        {
            var delivered = new T();

            var sourceProps = typeof (BlockchainTransaction).GetProperties().Where(x => x.CanRead && x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                try
                {
                    sourceProp.SetValue(delivered, sourceProp.GetValue(this, null), null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return delivered;
        }
    }
}