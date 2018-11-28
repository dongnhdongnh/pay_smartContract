using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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
        public static string GetSmartContractAbi()
        {
            string contents = File.ReadAllText(@"DN.Abi");
            Regex.Replace(contents, @"\t|\n|\r", "");
            contents.Replace("\"", "'");
            return contents;
        }
        public static string GetSmartContractAddress()
        {

            return "0x8070F3cba5833490b8794E2b4894b59A45cBbBa8";
        }
        public static string GetSmartContractPublicKey()
        {

            return "0xC2A213f481b2C794704218365E13d4761820B398";
        }
        public static string GetSmartContractPrivateKey()
        {

            return "08786bec5c4b2ac8cb6dcafb320a3486d59a0e9f81860792fe13ca0a962cda3e";
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