using System;
using System.Security.Cryptography;

namespace HB.FullStack.Common.IdGen
{
    public static class StaticIdGen
    {
        public static IDistributedLongIdGen IdGen { get; set; } = null!;

        public static long GetLongId()
        {
            return IdGen.GetId();
        }

        /// <summary>
        /// Not for SQL Server
        /// </summary>
        /// <param name="timeNow"></param>
        /// <param name="storeAsBinary">true as binary stored in database;otherwise, as string stored in database</param>
        /// <returns></returns>
        public static Guid GetSequentialGuid(GuidStoredFormat guidStoredFormat = GuidStoredFormat.AsBinary)
        {
            // According to RFC 4122:
            // dddddddd-dddd-Mddd-Ndrr-rrrrrrrrrrrr
            // - M = RFC version, in this case '4' for random UUID
            // - N = RFC variant (plus other bits), in this case 0b1000 for variant 1
            // - d = nibbles based on UTC date/time in ticks
            // - r = nibbles based on random bytes

            DateTimeOffset timeNow = DateTimeOffset.UtcNow;

            var randomBytes = new byte[7];
            SecurityUtil.RandomNumberGenerator.GetBytes(randomBytes);
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
    }
}
