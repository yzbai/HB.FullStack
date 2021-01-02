﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.Database;

namespace HB.FullStack.Mobile.IdBarriers
{
    public interface IIdBarrierRepo
    {
        Task AddIdBarrierAsync(IList<long> clientIds, IEnumerable<long> servierIds, TransactionContext transactionContext);
        Task AddIdBarrierAsync(long clientId, long serverId);
        Task<long> GetClientIdAsync(long serverId);
        Task<long> GetServerIdAsync(long clientId);
    }
}