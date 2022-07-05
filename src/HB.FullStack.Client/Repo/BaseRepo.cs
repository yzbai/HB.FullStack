﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Database;
//using Xamarin.Essentials;
//using Xamarin.Forms;
using Microsoft;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using HB.FullStack.Database.Entities;
using Microsoft.VisualStudio.Threading;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Client.ClientEntity;
using HB.FullStack.Client.Network;

namespace HB.FullStack.Client
{
    public delegate bool IfUseLocalData<TEntity>(ApiRequest request, IEnumerable<TEntity> entities) where TEntity : DatabaseEntity, new();

    public abstract class BaseRepo
    {
        protected IPreferenceProvider PreferenceProvider { get; }
        protected ConnectivityManager ConnectivityManager { get; }

        protected IApiClient ApiClient { get; }

        protected void EnsureLogined()
        {
            if (!PreferenceProvider.IsLogined())
            {
                throw ClientExceptions.NotLogined();
            }
        }

        protected bool IsInternetConnected(bool throwIfNot = true)
        {
            bool isInternetConnected = ConnectivityManager.IsInternet();

            if (throwIfNot && !isInternetConnected)
            {
                throw ClientExceptions.NoInternet("没有联网，且不允许离线");
            }

            return isInternetConnected;
        }

        protected void EnsureInternetConnected()
        {
            if (!ConnectivityManager.IsInternet())
            {
                throw ClientExceptions.NoInternet("没有联网");
            }
        }

        protected void EnsureNotSyncing()
        {
            if (ConnectivityManager.NeedSyncAfterReconnected)
            {
                throw ClientExceptions.OperationInvalidCauseofSyncingAfterReconnected();
            }
        }

        protected static void EnsureApiNotReturnNull([ValidatedNotNull][NotNull] object? obj, string entityName)
        {
            if (obj == null)
            {
                throw ApiExceptions.ServerNullReturn(parameter: entityName);
            }
        }

        protected BaseRepo(IApiClient apiClient, IPreferenceProvider userPreferenceProvider, ConnectivityManager connectivityManager)
        {
            ApiClient = apiClient;
            PreferenceProvider = userPreferenceProvider;
            ConnectivityManager = connectivityManager;
        }
    }

    public abstract class BaseRepo<TEntity, TRes> : BaseRepo where TEntity : DatabaseEntity, new() where TRes : ApiResource2
    {
        private readonly ILogger _logger;
        private readonly EntityDef _entityDef = null!;

        protected IDatabase Database { get; }

        protected ClientEntityDef ClientEntityDef { get; set; }

        protected BaseRepo(
            ILogger logger,
            IDatabase database,
            IApiClient apiClient,
            IPreferenceProvider userPreferenceProvider,
            ConnectivityManager connectivityManager) : base(apiClient, userPreferenceProvider, connectivityManager)
        {
            _logger = logger;
            _entityDef = database.EntityDefFactory.GetDef<TEntity>()!;

            Database = database;

            ClientEntityDef? clientEntityDef = ClientEntityDefFactory.Get<TEntity>();

            if (clientEntityDef == null)
            {
                clientEntityDef = CreateDefaultClientEntityDef();
            }

            ClientEntityDef = clientEntityDef;

            //NOTICE: Move this to options?
            static ClientEntityDef CreateDefaultClientEntityDef()
            {
                return new ClientEntityDef
                {
                    ExpiryTime = TimeSpan.FromSeconds(ClientEntityAttribute.DefaultExpirySeconds),
                    NeedLogined = true,
                    AllowOfflineRead = true,
                    AllowOfflineWrite = false
                };
            }
        }

        protected abstract TEntity ToEntity(TRes res);

        protected abstract TRes ToResource(TEntity entity);

        #region 查询

        protected async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TEntity>? ifUseLocalData = null)
        {
            EnsureNotSyncing();

            //TODO: await Syncing();

            //TODO: 是否应该由ClientEntity来决定需要login？或者由业务决定
            if (ClientEntityDef.NeedLogined)
            {
                _logger.LogDebug("检查Logined, Type:{type}", typeof(TEntity).Name);

                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{type}", typeof(TEntity).Name);
                return locals;
            }

            return await SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData).ConfigureAwait(false);
        }

        /// <summary>
        /// 先返回本地初始值，再更新为服务器值
        /// </summary>
        protected async Task<ObservableTask<IEnumerable<TEntity>>> GetObservableTaskAsync(
            Expression<Func<TEntity, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.Mixed,
            IfUseLocalData<TEntity>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            EnsureNotSyncing();

            if (ClientEntityDef.NeedLogined)
            {
                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<IEnumerable<TEntity>>(locals, null, onException, continueOnCapturedContext);
            }

            return new ObservableTask<IEnumerable<TEntity>>(
                locals,
                () => SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData),
                onException,
                continueOnCapturedContext);
        }

        protected async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TEntity>? ifUseLocalData = null)
        {
            EnsureNotSyncing();

            IEnumerable<TEntity> entities = await GetAsync(localWhere, remoteRequest, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);

            return entities.FirstOrDefault();
        }

        protected async Task<ObservableTask<TEntity?>> GetFirstOrDefaultObservableTaskAsync(
            Expression<Func<TEntity, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.Mixed,
            IfUseLocalData<TEntity>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            EnsureNotSyncing();

            if (ClientEntityDef.NeedLogined)
            {
                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<TEntity?>(locals.FirstOrDefault(), null, onException, continueOnCapturedContext);
            }

            if (ifUseLocalData == null)
            {
                ifUseLocalData = DefaultIfUseLocalData;
            }

            return new ObservableTask<TEntity?>(
                locals.FirstOrDefault(),
                async () => (await SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false)).FirstOrDefault(),
                onException,
                continueOnCapturedContext);
        }

        private async Task<IEnumerable<TEntity>> SyncGetAsync(
            IEnumerable<TEntity> localEntities,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TEntity> ifUseLocalData)
        {
            //如果不强制远程，并且满足使用本地数据条件
            if (getMode != RepoGetMode.RemoteForced && ifUseLocalData(remoteRequest, localEntities))
            {
                _logger.LogDebug("本地数据可用，返回本地, Type:{type}", typeof(TEntity).Name);
                return localEntities;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!IsInternetConnected(!ClientEntityDef.AllowOfflineRead))
            {
                _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{type}", typeof(TEntity).Name);

                ConnectivityManager.OnOfflineDataReaded();

                return localEntities;
            }

            //获取远程
            IEnumerable<TRes>? ress = await ApiClient.GetAsync<IEnumerable<TRes>>(remoteRequest).ConfigureAwait(false);

            IEnumerable<TEntity> remotes = ress!.Select(r => ToEntity(r)).ToList();

            _logger.LogDebug("远程数据获取完毕, Type:{type}", typeof(TEntity).Name);

            //TODO:
            //检查同步. 比如：离线创建的数据，现在联线，本地数据反而是新的。
            //多客户端：version相同，但lastuser不同。根据时间合并

            //单设备在线
            //单设备离线
            //多设备在线
            //多设备离线

            //情况：
            //1，（本地离线产生新数据）同一id数据，lastuser相同，local version 大于 remote version，使用本地更新远程
            //2，（多客户端）同一id数据，local version 等于 remote version，lastuser不同，按lasttime判断使用谁，如果local lasttime更大，使用本地更新远程，否则远程覆盖本地
            //3，同一id数据，local version 小于 remote version，覆盖本地

            //Case
            //1，第一个客户端，离线，疯狂update一条数据，将version变很大，然后第二个客户端在线，过了很久，update了同一条数据。现在第一个客户端在线，get这条数据

            //TODO: 这里不管不顾，直接用远程覆盖，是否要比较一下localEntities和remotes. Version的大小
            //如果本地Version大
            //如果本地Version小
            foreach (TEntity entity in remotes)
            {
                //TODO: Reset一遍，覆盖本地？
                await Database.SetByIdAsync(entity, transactionContext).ConfigureAwait(false);
            }

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{type}", typeof(TEntity).Name);

            return remotes;
        }

        /// <summary>
        /// 本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
        /// </summary>
        private bool DefaultIfUseLocalData(ApiRequest request, IEnumerable<TEntity> localEntities)
        {
            return localEntities.Any() && localEntities.All(t => TimeUtil.UtcNow - t.LastTime < ClientEntityDef.ExpiryTime);
        }

        #endregion

        #region 更改

        public async Task AddAsync(IEnumerable<TEntity> entities, TransactionContext transactionContext)
        {
            EnsureNotSyncing();

            ThrowIf.NotValid(entities, nameof(entities));

            if (!entities.Any())
            {
                return;
            }

            if (IsInternetConnected(!ClientEntityDef.AllowOfflineWrite))
            {
                //TODO: 这里的ApiRequestAuth从哪里获得?
                //Remote
                AddRequest<TRes> addRequest = new AddRequest<TRes>(entities.Select(k => ToResource(k)).ToList(), ApiRequestAuth.JWT, null);

                await ApiClient.SendAsync(addRequest).ConfigureAwait(false);

                //Local
                await Database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                throw new NotImplementedException();
                //允许脱网下写操作

                //Local
                //await Database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);

                //Record History
                //await Database.BatchAddAsync(GetOfflineHistories(entities, DbOperation.Add), "", transactionContext).ConfigureAwait(false);
            }
        }

        public async Task UpdateAsync(TEntity entity, TransactionContext transactionContext)
        {
            EnsureNotSyncing();

            ThrowIf.NotValid(entity, nameof(entity));

            if (IsInternetConnected(!ClientEntityDef.AllowOfflineWrite))
            {
                //TODO: 这里的ApiRequestAuth从哪里获得?
                UpdateRequest<TRes> updateRequest = new UpdateRequest<TRes>(ToResource(entity), ApiRequestAuth.JWT, null);

                //如果Version不对，会返回NotFount ErrorCode
                await ApiClient.SendAsync(updateRequest).ConfigureAwait(false);

                await Database.UpdateAsync(entity, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 允许脱网下写操作
                throw new NotImplementedException();
            }
        }

        private List<OfflineHistory> GetOfflineHistories(IEnumerable<TEntity> entities, DbOperation dbOperation)
        {
            List<OfflineHistory> histories = new List<OfflineHistory>(entities.Count());

            if (_entityDef.IsIdLong)
            {
                foreach (TEntity entity in entities)
                {
                    OfflineHistory history = new OfflineHistory
                    {
                        EntityId = (entity as LongIdEntity)!.Id.ToString(CultureInfo.InvariantCulture),
                        EntityFullName = _entityDef.EntityFullName,
                        Operation = dbOperation,
                        OperationTime = entity.LastTime,
                        Handled = false
                    };

                    histories.Add(history);
                }
            }
            else if (_entityDef.IsIdGuid)
            {
                foreach (TEntity entity in entities)
                {
                    OfflineHistory history = new OfflineHistory
                    {
                        EntityId = (entity as GuidEntity)!.Id.ToString(),
                        EntityFullName = _entityDef.EntityFullName,
                        Operation = dbOperation,
                        OperationTime = entity.LastTime,
                        Handled = false
                    };

                    histories.Add(history);
                }
            }
            else
            {
                throw ClientExceptions.UnSupportedEntityType(_entityDef.EntityFullName);
            }

            return histories;
        }

        #endregion
    }
}