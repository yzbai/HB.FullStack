

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public enum GuidStoredFormat
    {
        AsString = 0,
        AsBinary = 1
    }

    public static class SecurityUtil
    {
        //public static string GetSHA1(string str)
        //{
        //    SHA1 sha1 = SHA1.Create();
        //    byte[] buffer = Encoding.UTF8.GetBytes(str);

        //    byte[] sha1Bytes = sha1.ComputeHash(buffer);

        //    return DataConverter.ToHexString(sha1Bytes);

        //}

        //public static string GetMD5(string str)
        //{
        //    MD5 md5 = MD5.Create();
        //    byte[] buffer = Encoding.UTF8.GetBytes(str);
        //    byte[] md5Bytes = md5.ComputeHash(buffer);

        //    return DataConverter.ToHexString(md5Bytes);
        //    //return Convert.ToBase64String(md5Bytes);
        //}

        [ThreadStatic]
        private static SHA256? _sha256;

        public static SHA256 Sha256 => _sha256 ??= SHA256.Create();

        public static string GetHash(string item)
        {
            //using SHA256 sha256Obj = SHA256.Create();
            byte[] hashBytes = Sha256.ComputeHash(Encoding.UTF8.GetBytes(item));

            return Convert.ToBase64String(hashBytes);
        }

        public static string GetHash(IList<string> lst)
        {
            return GetHash(lst.ToJoinedString(null));
        }

        public static string GetHash<T>([DisallowNull] T item) where T : class
        {
            //using SHA256 sha256Obj = SHA256.Create();
            byte[] result = Sha256.ComputeHash(SerializeUtil.Serialize(item));

            return Convert.ToBase64String(result);
        }

        public static string EncryptPasswordWithSalt(string password, string salt)
        {
            return GetHash(password + salt);
        }

        public static string CreateUniqueToken()
        {
            return Guid.NewGuid().ToString("N", Globals.Culture);
        }

        #region Random String

        private const string CharCollection = "0,1,2,3,4,5,6,7,8,9,a,s,d,f,g,h,z,c,v,b,n,m,k,q,w,e,r,t,y,u,p,A,S,D,F,G,H,Z,C,V,B,N,M,K,Q,W,E,R,T,Y,U,P"; //定义验证码字符及出现频次 ,避免出现0 o j i l 1 x;
        private static readonly string[] _charArray = CharCollection.Split(',');
        private static readonly string[] _numbericCharArray = CharCollection.Substring(0, 10).Split(',');
        public static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

        public static string CreateRandomString(int length)
        {
            return CreateRandomString(length, _charArray);
        }

        public static string CreateRandomNumbericString(int length)
        {
            return CreateRandomString(length, _numbericCharArray);
        }

        private static string CreateRandomString(int length, string[] charArray)
        {
            //Random random = new Random(Guid.NewGuid().GetHashCode());
            int arrayLength = charArray.Length - 1;
            string randomString = "";

            for (int i = 0; i < length; i++)
            {
                randomString += charArray[GetRandomInteger(0, arrayLength - 1)];
            }

            return randomString;
        }

        /// <summary>
        /// [0.0, 1.0]
        /// </summary>
        public static double GetRandomDouble()
        {
            byte[] b = new byte[4];
            RandomNumberGenerator.GetBytes(b);
            return BitConverter.ToUInt32(b, 0) / (double)uint.MaxValue;
        }

        /// <summary>
        /// [minValue, maxValue]
        /// </summary>
        public static int GetRandomInteger(int minValue, int maxValue)
        {
#if NETSTANDARD2_0
            long range = (long)maxValue - minValue;

            return (int)((long)Math.Floor(GetRandomDouble() * range) + minValue);
#else
            return RandomNumberGenerator.GetInt32(minValue, maxValue + 1);
#endif
        }

        #endregion Random String

        public static byte[] HexToByteArray(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                string s = hexString.Substring(i, 2);
                bytes[i / 2] = byte.Parse(s, NumberStyles.HexNumber, null);
            }

            return bytes;
        }
    }
}