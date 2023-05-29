using System;

namespace HB.FullStack.KVStore
{
    /// <summary>
    /// 当KVStoreModel中没有显示使用KVStoreKeyAttribute标记key时，寻找此属性代表的 SubstituteKey
    /// 存在KVStoreKey时，此 SubstituteKey 不起作用
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KVStoreSubstituteKeyAttribute : System.Attribute
    {
        public KVStoreSubstituteKeyAttribute()
        {
        }
    }
}