using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public interface IBloomFilter
    {
        bool Exists(string bloomFilterName, string item);
        bool ExistAny(string bloomFilterName, string?[] items);
        void Delete(string bloomFilterName, string? oldItem);
        void Add(string bloomFilterName, string item);
        void Add(string bloomFilterName, string?[] items);

        //https://github.com/vla/BloomFilter.NetCore/blob/master/src/BloomFilter.Redis/RedisBitOperate.cs
    }
}
