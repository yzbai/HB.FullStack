using System;
using System.Threading.Tasks;

namespace HB.FullStack.CacheTests
{
    public class DatabaseMocker
    {
        public string Guid { get; set; }
        public long InitialTimestamp = TimeUtil.UtcNowTicks;

        public DatabaseMocker(string guid)
        {
            Guid = guid;
        }

        public async Task<VersionData> RetrieveAsync()
        {
            await Task.Delay(10);
            return new VersionData { Guid = Guid, Timestamp = InitialTimestamp};
        }

        public async Task UpdateAsync(VersionData versionData)
        {
            await Task.Delay(20);
            versionData.Timestamp = TimeUtil.UtcNowTicks;
        }
    }
}