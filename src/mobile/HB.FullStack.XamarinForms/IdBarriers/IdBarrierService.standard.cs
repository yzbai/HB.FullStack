using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.XamarinForms.IdBarriers;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Database;
using HB.FullStack.Common;
using HB.FullStack.Common.ApiClient;
using System.Net.Http;

namespace HB.FullStack.XamarinForms.IdBarriers
{

    internal class IdBarrierService : IIdBarrierService
    {
        enum ChangeDirection
        {
            ToServer,
            FromServer
        }

        private readonly IIdBarrierRepo _idBarrierRepo;
        private readonly IApiClient _apiClient;
        private readonly ITransaction _transaction;
        private readonly Dictionary<string, List<long>> _addRequestClientIdDict = new Dictionary<string, List<long>>();

        public IdBarrierService(IIdBarrierRepo idBarrierRepo, IApiClient apiClient, ITransaction transaction)
        {
            _idBarrierRepo = idBarrierRepo;
            _apiClient = apiClient;
            _transaction = transaction;
        }

        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="MobileException"></exception>
        public void Initialize()
        {
            _apiClient.Requesting += OnApiClientRequestingAsync;
            _apiClient.Responsed += OnApiClientResponsedAsync;
        }

        //TODO: 考虑手工硬编码
        /// <exception cref="MobileException"></exception>
        /// <exception cref="DatabaseException"></exception>
        private async Task OnApiClientRequestingAsync(ApiRequest request, ApiEventArgs args)
        {
            if (request.HttpMethod == HttpMethod.Post)
            {
                _addRequestClientIdDict[request.RequestId] = new List<long>();
            }

            await ChangeIdAsync(request, request.RequestId, request.HttpMethod, ChangeDirection.ToServer).ConfigureAwait(false);
        }

        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="MobileException"></exception>
        private async Task OnApiClientResponsedAsync(object? sender, ApiEventArgs args)
        {
            if(args.RequestHttpMethod == HttpMethod.Post)
            {
                if (sender is IEnumerable<long> servierIds)
                {
                    List<long> clientIds = _addRequestClientIdDict[args.RequestId];

                    await AddServerIdToClientIdAsync(servierIds, clientIds).ConfigureAwait(false);

                    _addRequestClientIdDict.Remove(args.RequestId);
                }
            }
            else if(args.RequestHttpMethod == HttpMethod.Get)
            {
                if (sender is IEnumerable enumerable)
                {
                    foreach (object obj in enumerable)
                    {
                        await ChangeIdAsync(obj, args.RequestId, args.RequestHttpMethod, ChangeDirection.FromServer).ConfigureAwait(false);
                    }
                }
                else
                {
                    await ChangeIdAsync(sender, args.RequestId, args.RequestHttpMethod, ChangeDirection.FromServer).ConfigureAwait(false);
                }
            }
        }

        /// <exception cref="MobileException"></exception>
        /// <exception cref="DatabaseException"></exception>
        private async Task ChangeIdAsync(object? obj, string requestId, HttpMethod httpMethod, ChangeDirection direction)
        {
            if (obj == null) { return; }

            //替换ID
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(IdBarrierAttribute))))
            {
                object? propertyValue = propertyInfo.GetValue(obj);

                if (propertyValue == null)
                {
                    continue;
                }

                if (propertyValue is long id)
                {
                    await ConvertLongIdAsync(obj, id, propertyInfo, httpMethod, direction, requestId).ConfigureAwait(false);
                }
                else if (propertyValue is IEnumerable<long> longIds)
                {
                    foreach (long iditem in longIds)
                    {
                        await ConvertLongIdAsync(obj, iditem, propertyInfo, httpMethod, direction, requestId).ConfigureAwait(false);
                    }
                }
                else if (propertyValue is IEnumerable enumerable)
                {
                    foreach (object subObj in enumerable)
                    {
                        await ChangeIdAsync(subObj, requestId, httpMethod, direction).ConfigureAwait(false);
                    }
                }
                else if (propertyInfo.PropertyType.IsClass)
                {
                    await ChangeIdAsync(propertyValue, requestId, httpMethod, direction).ConfigureAwait(false);
                }
                else
                {
                    throw MobileExceptions.IdBarrierError(cause: "Id Barrier碰到无法解析的类型");
                }
            }
        }

        /// <exception cref="DatabaseException"></exception>
        private async Task ConvertLongIdAsync(object obj, long id, PropertyInfo propertyInfo, HttpMethod httpMethod, ChangeDirection direction, string requestId)
        {
            if (id < 0)
            {
                return;
            }

            if (propertyInfo.Name == nameof(LongIdResource.Id) && httpMethod == HttpMethod.Post && direction == ChangeDirection.ToServer)
            {
                _addRequestClientIdDict[requestId].Add(id);

                propertyInfo.SetValue(obj, -1);

                return;
            }

            long changedId = direction switch
            {
                ChangeDirection.ToServer => await _idBarrierRepo.GetServerIdAsync(id).ConfigureAwait(false),
                ChangeDirection.FromServer => await _idBarrierRepo.GetClientIdAsync(id).ConfigureAwait(false),
                _ => -1,
            };

            if (direction == ChangeDirection.FromServer
                && changedId < 0
                && id > 0
                && httpMethod == HttpMethod.Get)
            {
                changedId = StaticIdGen.GetId();
                await AddServerIdToClientIdAsync(id, changedId).ConfigureAwait(false);
            }
            //TODO: 如果服务器返回Id=-1，即这个数据不是使用Id来作为主键的，客户端实体应该避免使用IdGenEntity

            propertyInfo.SetValue(obj, changedId);
        }

        /// <exception cref="DatabaseException"></exception>
        private Task AddServerIdToClientIdAsync(long serverId, long clientId)
        {
            if (serverId <= 0)
            {
                return Task.CompletedTask;
            }

            return _idBarrierRepo.AddIdBarrierAsync(clientId: clientId, serverId: serverId);
        }

        /// <exception cref="DatabaseException"></exception>
        private async Task AddServerIdToClientIdAsync(IEnumerable<long> serverIds, List<long> clientIds)
        {
            List<long> serverAdds = new List<long>();
            List<long> clientAdds = new List<long>();

            int num = 0;

            foreach (long serverId in serverIds)
            {
                if (serverId <= 0)
                {
                    continue;
                }

                serverAdds.Add(serverId);
                clientAdds.Add(clientIds[num++]);
            }

            TransactionContext trans = await _transaction.BeginTransactionAsync<IdBarrier>().ConfigureAwait(false);
            try
            {
                await _idBarrierRepo.AddIdBarrierAsync(clientIds: clientAdds, servierIds: serverAdds, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
