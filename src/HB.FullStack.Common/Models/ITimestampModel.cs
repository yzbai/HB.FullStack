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
        //int Version { get; set; }
        long Timestamp { get; set; }
    }
}