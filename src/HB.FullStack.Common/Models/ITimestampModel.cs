using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]
namespace HB.FullStack.Common
{
    public interface ITimestamp
    {
        /// <summary>
        /// 取代Version，实现行粒度。
        /// Version存在UserA将Version为1大老数据更改两次得到Version3，UserB将Version为2的数据更改一次变成Version3，都是version3，但经过路径不同，但系统认为相同。
        /// 就把Timestamp看作Version就行
        /// </summary>
        long Timestamp { get; set; }
    }
}