using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Client.Api;
using HB.FullStack.Client.IdBarriers;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Common.Resources;

namespace MyColorfulTime.IdBarriers
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

        private readonly Dictionary<string, List<long>> _addRequestClientIdDict = new Dictionary<string, List<long>>();

        public IdBarrierService(IIdBarrierRepo idBarrierRepo, IApiClient apiClient)
        {
            _idBarrierRepo = idBarrierRepo;
            _apiClient = apiClient;
        }

        public void Initialize()
        {
            _apiClient.Requesting += ApiClient_RequestingAsync;
            _apiClient.Responsed += ApiClient_ResponsedAsync;
        }

        //考虑手工硬编码
        private async Task ApiClient_RequestingAsync(ApiRequest request, ApiEventArgs args)
        {
            if (args.RequestType == ApiRequestType.Add)
            {
                _addRequestClientIdDict[request.GetRequestId()] = new List<long>();
            }

            await ChangeIdAsync(request, args.RequestId, ChangeDirection.ToServer, args.RequestType).ConfigureAwait(false);
        }

        private async Task ApiClient_ResponsedAsync(object? sender, ApiEventArgs args)
        {
            switch (args.RequestType)
            {
                case ApiRequestType.Add:

                    if (sender is IEnumerable<long> servierIds)
                    {
                        List<long> clientIds = _addRequestClientIdDict[args.RequestId];

                        await _idBarrierRepo.AddIdBarrierAsync(clientIds, servierIds).ConfigureAwait(false);

                        _addRequestClientIdDict.Remove(args.RequestId);
                    }

                    break;
                case ApiRequestType.Update:
                    break;
                case ApiRequestType.Delete:
                    break;
                case ApiRequestType.Get:
                    if (sender is IEnumerable enumerable)
                    {
                        foreach (object obj in enumerable)
                        {
                            await ChangeIdAsync(obj, args.RequestId, ChangeDirection.FromServer, args.RequestType).ConfigureAwait(false);
                        }
                    }
                    break;
                case ApiRequestType.GetSingle:
                    await ChangeIdAsync(sender, args.RequestId, ChangeDirection.FromServer, args.RequestType).ConfigureAwait(false);
                    break;
                default:
                    break;
            }
        }

        private async Task ChangeIdAsync(object? obj, string requestId, ChangeDirection direction, ApiRequestType requestType)
        {
            if (obj == null) { return; }

            //替换ID
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(BindingFlags.Public))
            {
                if (!Attribute.IsDefined(propertyInfo, typeof(IdBarrierAttribute)))
                {
                    continue;
                }

                object? propertyValue = propertyInfo.GetValue(obj);

                if (propertyValue == null)
                {
                    continue;
                }

                if (propertyValue is long id)
                {
                    if (id < 0)
                    {
                        continue;
                    }

                    if (propertyInfo.Name == nameof(Resource.Id) && requestType == ApiRequestType.Add && direction == ChangeDirection.ToServer)
                    {
                        _addRequestClientIdDict[requestId].Add(id);

                        propertyInfo.SetValue(obj, -1);
                    }

                    long changedId = direction switch
                    {
                        ChangeDirection.ToServer => await _idBarrierRepo.GetServerIdAsync(id).ConfigureAwait(false),
                        ChangeDirection.FromServer => await _idBarrierRepo.GetClientIdAsync(id).ConfigureAwait(false),
                        _ => -1,
                    };

                    if (changedId < 0 &&
                        propertyInfo.Name == nameof(Resource.Id) &&
                        (requestType == ApiRequestType.Get || requestType == ApiRequestType.GetSingle) &&
                        direction == ChangeDirection.FromServer)
                    {
                        changedId = IDistributedIdGen.IdGen.GetId();
                        await _idBarrierRepo.AddIdBarrierAsync(clientId: changedId, serverId: id).ConfigureAwait(false);
                    }

                    propertyInfo.SetValue(obj, changedId);

                    continue;
                }
                else if (propertyValue is IEnumerable enumerable)
                {
                    foreach (object subObj in enumerable)
                    {
                        await ChangeIdAsync(subObj, requestId, direction, requestType).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new ClientException($"Id Barrier碰到无法解析的类型");
                }
            }
        }
    }
}
