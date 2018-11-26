using System;

namespace SmartContract.Commons.Constants
{
    public class RedisCacheKey
    {
        // Dashboard's cachekey
        public const string COINMARKET_PRICE_CACHEKEY = "price_{0}_{1}";

        //CacheHelper
        public const String KEY_SCANBLOCK_LASTSCANBLOCK = "KEY_{0}_LASTSCANBLOCK";
    }
}