using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Database;

namespace HB.FullStack.Mobile.IdBarriers
{
    //TODO: 考虑缓存
    internal class IdBarrierRepo : IIdBarrierRepo
    {
        private readonly IDatabase _database;

        public IdBarrierRepo(IDatabase database)
        {
            _database = database;
        }

        public async Task AddIdBarrierAsync(long clientId, long serverId)
        {
            IdBarrier idBarrier = new IdBarrier { ClientId = clientId, ServerId = serverId };

            await _database.AddAsync(idBarrier, "", null).ConfigureAwait(false);
        }

        public async Task AddIdBarrierAsync(IList<long> clientIds, IEnumerable<long> servierIds, TransactionContext transactionContext)
        {
            List<IdBarrier> idBarriers = new List<IdBarrier>();

            int num = 0;

            foreach (long servierId in servierIds)
            {
                IdBarrier idBarrier = new IdBarrier { ClientId = clientIds[num++], ServerId = servierId };

                idBarriers.Add(idBarrier);
            }

            await _database.BatchAddAsync(idBarriers, "", transactionContext).ConfigureAwait(false);
        }

        public async Task<long> GetClientIdAsync(long serverId)
        {
            IdBarrier? barrier = await _database.ScalarAsync<IdBarrier>(item => item.ServerId == serverId, null).ConfigureAwait(false);

            if (barrier == null)
            {
                return -1;
            }

            return barrier.ClientId;
        }

        public async Task<long> GetServerIdAsync(long clientId)
        {
            IdBarrier? barrier = await _database.ScalarAsync<IdBarrier>(item => item.ClientId == clientId, null).ConfigureAwait(false);

            if (barrier == null)
            {
                return -1;
            }

            return barrier.ServerId;
        }
    }
}
