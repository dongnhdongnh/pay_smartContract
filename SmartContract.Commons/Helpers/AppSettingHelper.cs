using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using SmartContract.Commons.Constants;

namespace SmartContract.Commons.Helpers
{
    public class AppSettingHelper
    {
        private static IConfiguration Configuration { get; }

        static AppSettingHelper()
        {
            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
                environment = "Development";
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();
        }

        public static string Get(string sectionKey)
        {
            return Configuration[sectionKey];
        }

        public static string GetDbConnection()
        {
            return Configuration.GetConnectionString(Setting.SECTION_KEY_SQL);
        }

        public static string GetNodeSetting(string currency, string section = Setting.SECTION_KEY_URL)
        {
            return Get($"{Setting.SECTION_KEY_CHAIN}:{currency}:{section}");
        }

        public static string GetVakacoinNode()
        {
            return GetNodeSetting(CryptoCurrency.VAKA);
        }

        public static string GetVakacoinBlockInterval()
        {
            return GetNodeSetting(CryptoCurrency.VAKA, Setting.SECTION_KEY_BLOCK_INTERVAL);
        }

        public static string GetBitcoinNode()
        {
            return GetNodeSetting(CryptoCurrency.BTC);
        }

        public static NetworkCredential GetBitcoinRpcAuthentication()
        {
            return new NetworkCredential(GetNodeSetting(CryptoCurrency.BTC, Setting.SECTION_KEY_USERNAME),
                GetNodeSetting(CryptoCurrency.BTC, Setting.SECTION_KEY_PASSWORD));
        }

        public static string GetEthereumNode()
        {
            return GetNodeSetting(CryptoCurrency.ETH);
        }

        public static string GetRedisConfig()
        {
            return Get($"{Setting.SECTION_KEY_CACHE}:{Setting.SECTION_KEY_URL}");
        }

        public static string GetElasticMailUrl()
        {
            return Get($"{Setting.SECTION_KEY_ELASTIC}:{Setting.SECTION_KEY_EMAIL_URL}");
        }

        public static string GetElasticSmsUrl()
        {
            return Get($"{Setting.SECTION_KEY_ELASTIC}:{Setting.SECTION_KEY_SMS_URL}");
        }

        public static string GetElasticApiKey()
        {
            return Get($"{Setting.SECTION_KEY_ELASTIC}:{Setting.SECTION_KEY_API_KEY}");
        }

        public static string GetImgurApiKey()
        {
            return Get($"{Setting.SECTION_KEY_IMGUR}:{Setting.SECTION_KEY_IMG_API_KEY}");
        }

        public static string GetImgurUrl()
        {
            return Get($"{Setting.SECTION_KEY_IMGUR}:{Setting.SECTION_KEY_URL_IMG}");
        }

        public static string GetElasticFromAddress()
        {
            return Get($"{Setting.SECTION_KEY_ELASTIC}:{Setting.SECTION_KEY_FROM_ADDRESS}");
        }

        public static string GetElasticFromName()
        {
            return Get($"{Setting.SECTION_KEY_ELASTIC}:{Setting.SECTION_KEY_FROM_NAME}");
        }

        public static string GetCoinMarketUrl()
        {
            return Get($"{Setting.SECTION_KEY_COIN_MARKET}:{Setting.SECTION_KEY_URL}");
        }

        public static string GetReportStoreUrl()
        {
            return Get($"{Setting.SECTION_KEY_REPORT_STORE}:{Setting.SECTION_KEY_URL}");
        }

        public static int GetCoinMarketInterval()
        {
            return Int32.Parse(Get($"{Setting.SECTION_KEY_COIN_MARKET}:{Setting.SECTION_KEY_GET_PRICE_INTERVAL}"));
        }

        public static string GetCurrencyConverterUrl()
        {
            return Get($"{Setting.SECTION_KEY_CURRENCY_CONVERTER_API}:{Setting.SECTION_KEY_URL}");
        }

        public static int GetCurrencyConverterInterval()
        {
            return Int32.Parse(Get(
                $"{Setting.SECTION_KEY_CURRENCY_CONVERTER_API}:{Setting.SECTION_KEY_GET_CURRENCY_CONVERTER_INTERVAL}"));
        }
    }
}