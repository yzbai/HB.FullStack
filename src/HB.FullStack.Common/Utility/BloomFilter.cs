using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HB.FullStack.Common
{
    /// <summary>Represents a probabilistic data structure that is optimal to
    /// determine if an object isn't or may be present in a set.</summary>
    public class BloomFilter
    {
        private readonly IHashFunction _hash = new MurMurHash3();

        private readonly int _k;
        private readonly int _m;

        private BitArray _filter;

        /// <summary>Initialized a new instance of <see cref="BloomFilter"/> with the specified desired capacity and false-positive probability.</summary>
        /// <param name="n">The approximate amount of objects that the filter will contain.</param>
        /// <param name="p">The false-positive probability.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> or <paramref name="p"/> is out of range.</exception>
        public BloomFilter(int n, double p)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), n, "n cannot be negative.");
            }

            if (p <= 0 || p >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(p), p, "p must be within this range: (0, 1).");
            }

            _m = EvaluateM(n, p);
            _k = EvaluateK(_m, n);

            _filter = new BitArray(_m);
        }

        /// <summary>Initialized a new instance of <see cref="BloomFilter"/> with the specified width and depth.</summary>
        /// <param name="m">The length of the array implementing the filter.</param>
        /// <param name="k">The number of hash functions to apply.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="m"/> or <paramref name="k"/> is negative.</exception>
        public BloomFilter(int m, int k)
        {
            if (m < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(m), m, "m cannot be negative.");
            }

            if (k < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(k), k, "k cannot be negative.");
            }

            _m = m;
            _k = k;

            _filter = new BitArray(_m);
        }

        /// <summary>Adds an object to the <see cref="BloomFilter"/>.
        /// <code>Complexity: O(1)</code></summary>
        /// <param name="obj">The object to add to the <see cref="BloomFilter"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        public void Add(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var hashes = GetHashes(obj, _m, _k);

            for (int i = 0; i < _k; i++)
            {
                _filter[hashes[i]] = true;
            }
        }

        /// <summary>Determines if the specified object isn't or may be in the <see cref="BloomFilter"/>.
        /// <code>Complexity: O(1)</code></summary>
        /// <param name="obj">The object to locate in the <see cref="BloomFilter"/>.</param>
        /// <returns><c>false</c> if <paramref name="obj"/> is definitely not in the <see cref="BloomFilter"/>, <c>true</c> if it may be.</returns>
        public bool Contains(object obj)
        {
            var hashes = GetHashes(obj, _m, _k);

            for (int i = 0; i < _k; i++)
            {
                if (!_filter[hashes[i]])
                    return false;
            }

            return true;
        }

        /// <summary>Removes all objects from the <see cref="BloomFilter"/>.</summary>
        public void Clear()
        {
            _filter = new BitArray(_m);
        }

        #region Helpers

        private static int EvaluateM(double n, double p) => (int)Math.Ceiling(-n * Math.Log(p) / Math.Pow(Math.Log(2), 2));

        private static int EvaluateK(double m, double n) => (int)Math.Round((m / n) * Math.Log(2));

        // https://en.wikipedia.org/wiki/Double_hashing
        private int[] GetHashes(object obj, int maxValue, int count)
        {
            int hash1 = obj.GetHashCode();
            int hash2 = GetHash(obj);

            var array = new int[count];

            for (int i = 0; i < count; i++)
            {
                unchecked
                {
                    array[i] = Math.Abs(hash1 + hash2 * (i + 1)) % maxValue;
                }
            }

            return array;
        }

        private int GetHash(object obj)
        {
            int output;
            using (var stream = new MemoryStream(ObjectToStream(obj)))
            {
                output = _hash.Hash(stream);
            }

            return output;
        }

        private static byte[]? ObjectToStream(object obj)
        {
            if (obj == null)
                return null;

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        #endregion
    }

    interface IHashFunction
    {
        int Hash(Stream stream);
    }

    // https://gist.github.com/automatonic/3725443
    class MurMurHash3 : IHashFunction
    {
        private readonly uint _seed = 144;

        public MurMurHash3()
        {
        }

        public MurMurHash3(uint seed)
        {
            _seed = seed;
        }

        public int Hash(Stream stream)
        {
            const uint C1 = 0xcc9e2d51;
            const uint C2 = 0x1b873593;

            uint h1 = _seed;
            uint streamLength = 0;
            using (var reader = new BinaryReader(stream))
            {
                var chunk = reader.ReadBytes(4);
                while (chunk.Length > 0)
                {
                    streamLength += (uint)chunk.Length;
                    uint k1;
                    switch (chunk.Length)
                    {
                        case 4:
                            k1 = (uint)
                                (chunk[0]
                               | chunk[1] << 8
                               | chunk[2] << 16
                               | chunk[3] << 24);

                            k1 *= C1;
                            k1 = Rotl32(k1, 15);
                            k1 *= C2;

                            h1 ^= k1;
                            h1 = Rotl32(h1, 13);
                            h1 = h1 * 5 + 0xe6546b64;
                            break;
                        case 3:
                            k1 = (uint)
                                (chunk[0]
                               | chunk[1] << 8
                               | chunk[2] << 16);
                            k1 *= C1;
                            k1 = Rotl32(k1, 15);
                            k1 *= C2;
                            h1 ^= k1;
                            break;
                        case 2:
                            k1 = (uint)
                                (chunk[0]
                               | chunk[1] << 8);
                            k1 *= C1;
                            k1 = Rotl32(k1, 15);
                            k1 *= C2;
                            h1 ^= k1;
                            break;
                        case 1:
                            k1 = chunk[0];
                            k1 *= C1;
                            k1 = Rotl32(k1, 15);
                            k1 *= C2;
                            h1 ^= k1;
                            break;

                    }
                    chunk = reader.ReadBytes(4);
                }
            }

            h1 ^= streamLength;
            h1 = Fmix(h1);

            unchecked
            {
                return (int)h1;
            }
        }

        private static uint Rotl32(uint x, byte r) => (x << r) | (x >> (32 - r));

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}
