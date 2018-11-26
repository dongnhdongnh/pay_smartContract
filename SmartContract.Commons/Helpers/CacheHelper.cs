using System;
using StackExchange.Redis;

namespace SmartContract.Commons.Helpers
{
    public class CacheHelper
    {
        static CacheHelper()
        {
            try
            {
                lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                {
                    Console.WriteLine("GET REDIS FROM " + AppSettingHelper.GetRedisConfig());
                    return ConnectionMultiplexer.Connect($"{AppSettingHelper.GetRedisConfig()}");
                });
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
           

        }

        private static Lazy<ConnectionMultiplexer> lazyConnection;

        private static ConnectionMultiplexer Connection
        {
            get { return lazyConnection.Value; }
        }

        private static IDatabase CacheDatabase
        {
            get { return Connection.GetDatabase(); }
        }

        public static void SetCacheString(String key, String value)
        {
            CacheDatabase.StringSet(key, value);
        }

        public static bool HaveKey(RedisKey key)
        {
            return CacheDatabase.KeyExists(key);
        }

        public static String GetCacheString(String key)
        {
            Console.WriteLine("get " + key);
            return CacheDatabase.StringGet(key);
        }

        public static bool DeleteCacheString(String key)
        {
            return CacheDatabase.KeyDelete(key);
        }
    }
}