using System;
using System.Threading.Tasks;

namespace HB.FullStack.CacheTests
{
    public class DatabaseMocker
    {
        public int CurrentVerson = 1;
        public string Guid = SecurityUtil.CreateUniqueToken();

        public async Task<VersionData> RetrieveAsync()
        {
            await Task.Delay(10);
            return new VersionData { Guid = Guid, Version = CurrentVerson };
        }

        public async Task UpdateAsync(VersionData versionData)
        {
            await Task.Delay(20);
            CurrentVerson = versionData.Version;
        }
    }
}