using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.IdBarriers
{
    public interface IIdBarrierRepo
    {
        Task AddIdBarrierAsync(List<long> clientIds, IEnumerable<long> servierIds);
        Task AddIdBarrierAsync(long clientId, long serverId);
        Task<long> GetClientIdAsync(long serverId);
        Task<long> GetServerIdAsync(long clientId);
    }
}