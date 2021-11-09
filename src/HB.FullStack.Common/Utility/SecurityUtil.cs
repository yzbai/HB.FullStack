#nullable enable

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

        /// <summary>
        /// GetHash
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>


        public static string GetHash(string item)
        {
            using SHA256 sha256Obj = SHA256.Create();
            byte[] hashBytes = sha256Obj.ComputeHash(Encoding.UTF8.GetBytes(item));

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// GetHash
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>




        public static async Task<string> GetHashAsync<T>([DisallowNull] T item) where T : class
        {
            using SHA256 sha256Obj = SHA256.Create();
            byte[] result = sha256Obj.ComputeHash(await SerializeUtil.PackAsync(item).ConfigureAwait(false));

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// EncryptPwdWithSalt
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="salt"></param>
        /// <returns></returns>


        public static string EncryptPwdWithSalt(string pwd, string salt)
        {
            return GetHash(pwd + salt);
        }

        public static string CreateUniqueToken()
        {
            return Guid.NewGuid().ToString("N", GlobalSettings.Culture);
        }

        

        /// <summary>
        /// Not for SQL Server
        /// </summary>
        /// <param name="timeNow"></param>
        /// <param name="storeAsBinary">true as binary stored in database;otherwise, as string stored in database</param>
        /// <returns></returns>
        public static Guid CreateSequentialGuid(DateTimeOffset timeNow, GuidStoredFormat guidStoredFormat)
        {
            // According to RFC 4122:
            // dddddddd-dddd-Mddd-Ndrr-rrrrrrrrrrrr
            // - M = RFC version, in this case '4' for random UUID
            // - N = RFC variant (plus other bits), in this case 0b1000 for variant 1
            // - d = nibbles based on UTC date/time in ticks
            // - r = nibbles based on random bytes

            var randomBytes = new byte[7];
            _randomNumberGenerator.GetBytes(randomBytes);
            var ticks = (ulong)timeNow.Ticks;

            var uuidVersion = (ushort)4;
            var uuidVariant = (ushort)0b1000;

            var ticksAndVersion = (ushort)((ticks << 48 >> 52) | (ushort)(uuidVersion << 12));
            var ticksAndVariant = (byte)((ticks << 60 >> 60) | (byte)(uuidVariant << 4));

            if (guidStoredFormat == GuidStoredFormat.AsBinary)
            {
                var guidBytes = new byte[16];
                var tickBytes = BitConverter.GetBytes(ticks);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(tickBytes);
                }

                Buffer.BlockCopy(tickBytes, 0, guidBytes, 0, 6);
                guidBytes[6] = (byte)(ticksAndVersion << 8 >> 8);
                guidBytes[7] = (byte)(ticksAndVersion >> 8);
                guidBytes[8] = ticksAndVariant;
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 9, 7);

                return new Guid(guidBytes);
            }

            var guid = new Guid((uint)(ticks >> 32), (ushort)(ticks << 32 >> 48), ticksAndVersion,
                ticksAndVariant,
                randomBytes[0],
                randomBytes[1],
                randomBytes[2],
                randomBytes[3],
                randomBytes[4],
                randomBytes[5],
                randomBytes[6]);

            return guid;
        }

        #region Random String

        private const string _charCollection = "0,1,2,3,4,5,6,7,8,9,a,s,d,f,g,h,z,c,v,b,n,m,k,q,w,e,r,t,y,u,p,A,S,D,F,G,H,Z,C,V,B,N,M,K,Q,W,E,R,T,Y,U,P"; //定义验证码字符及出现频次 ,避免出现0 o j i l 1 x;
        private static readonly string[] _charArray = _charCollection.Split(',');
        private static readonly string[] _numbericCharArray = _charCollection[..20].Split(',');
        private static readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();
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
            _randomNumberGenerator.GetBytes(b);
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
#endif
#if NETSTANDARD2_1 || NET6_0
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