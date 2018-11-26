using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartContract.Commons.Helpers
{
    public static class CommonHelper
    {
        public static string GenerateUuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static long GetUnixTimestamp()
        {
            return UnixTimestamp.ToUnixTimestamp(DateTime.UtcNow);
        }


        public static bool ValidateId(string id)
        {
            const string pattern = "^([0-9a-k]{8}[-][0-9a-k]{4}[-][0-9a-k]{4}[-][0-9a-k]{4}[-][0-9a-k]{12})$";
            return Regex.IsMatch(id, pattern);
        }

        /// <summary>
        /// Generate Token Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="apiSecret"></param>
        /// <param name="timeStamp"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GenerateTokenKey(string apiKey, string apiSecret, string timeStamp, string path)
        {
            try
            {
                string hashLeft;
                string hashRight;
                var key = string.Join(":", apiSecret, apiKey, timeStamp, path);
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
                {
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(string.Join(":", apiSecret, key)));
                    hashLeft = Convert.ToBase64String(hmac.Hash);
                    hashRight = string.Join(":", apiKey, timeStamp);
                }

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(":", hashLeft, hashRight)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string Md5(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }


        public static bool ValidateGuid(string guidString)
        {
            return Guid.TryParse(guidString, out var _);
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomAccountNameVakacoin(int length = 12)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz12345";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string IntToHex(this int input, string extra = null)
        {
            if (extra == null)
                return "0x" + input.ToString("X");
            else
                return extra + input.ToString("X");
        }

        public static string ToHex(this BigInteger input, string extra = null)
        {
            if (extra == null)
                return "0x" + input.ToString("X").TrimStart(new char[] { '0' } );
            else
                return extra + input.ToString("X").TrimStart(new char[] { '0' } );
        }

        public static bool HexToInt(this string hex, out int result)
        {
            char[] trimHex = new char[] {'0', 'x'};
            return int.TryParse(hex.TrimStart(trimHex), System.Globalization.NumberStyles.HexNumber, null,
                out result);
        }

        public static bool HexToBigInteger(this string hex, out BigInteger result)
        {
            char[] trimHex = new char[] {'0', 'x'};
            return BigInteger.TryParse(hex.TrimStart(trimHex), System.Globalization.NumberStyles.HexNumber, null,
                out result);
        }

        public static string GetPropertyName<T, P>(Expression<Func<T, P>> propertyDelegate)
        {
            var expression = (MemberExpression)propertyDelegate.Body;
            return expression.Member.Name;
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}