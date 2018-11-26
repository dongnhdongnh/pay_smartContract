using System.Collections.Generic;

namespace SmartContract.Commons.Constants
{
    public class CryptoCurrency
    {
        public const string ETH = "Ethereum";
        public const string VAKA = "Vakacoin";
        public const string BTC = "Bitcoin";

        public static readonly IEnumerable<string> ALL_NETWORK = new string[] {ETH, VAKA, BTC};

        private static readonly Dictionary<string, string> SYMBOLS = new Dictionary<string, string>
        {
            {ETH, "ETH"},
            {VAKA, "VAKA"},
            {BTC, "BTC"}
        };

        public static string GetAmount(string currency, decimal amount)
        {
            if (currency == VAKA)
            {
                return amount.ToString("N4") + " " + SYMBOLS[currency];
            }

            return amount + " " + SYMBOLS[currency];
        }
    }
}