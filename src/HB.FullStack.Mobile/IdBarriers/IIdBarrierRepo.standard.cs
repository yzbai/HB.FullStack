using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.Database;
using System;

namespace HB.FullStack.Mobile.IdBarriers
{
    public interface IIdBarrierRepo
    {
        /// <exception cref="DatabaseException"></exception>
        Task AddIdBarrierAsync(IList<long> clientIds, IEnumerable<long> servierIds, TransactionContext transactionContext);

        /// <exception cref="DatabaseException"></exception>
        Task AddIdBarrierAsync(long clientId, long serverId);

        /// <exception cref="DatabaseException"></exception>
        Task<long> GetClientIdAsync(long serverId);

        /// <exception cref="DatabaseException"></exception>
        Task<long> GetServerIdAsync(long clientId);
    }
}