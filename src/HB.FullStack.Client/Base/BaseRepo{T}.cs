/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.Sync;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Client.Base
{
    public abstract class BaseRepo<TModel> : BaseRepo where TModel : ClientDbModel, new()
    {
        private readonly ILogger _logger;
        private readonly IClientModelSettingFactory _clientModelSettingFactory;
        private readonly ISyncManager _syncManager;
        private readonly IClientEvents _clientEvents;
        private readonly DbModelDef _modelDef = null!;

        protected IDatabase Database { get; }

        protected ClientModelSetting ClientModelSetting { get; set; }

        protected BaseRepo(
            ILogger logger,
            IClientModelSettingFactory clientModelSettingFactory,
            IDatabase database,
            IApiClient apiClient,
            ISyncManager syncManager,
            IClientEvents clientEvents,
            ITokenPreferences clientPreferences) : base(apiClient, clientPreferences)
        {
            _logger = logger;
            _clientModelSettingFactory = clientModelSettingFactory;
            _modelDef = database.ModelDefFactory.GetDef<TModel>()!;

            ClientModelSetting = _clientModelSettingFactory.Get<TModel>() ?? new ClientModelSetting();
            Database = database;
            _syncManager = syncManager;
            _clientEvents = clientEvents;
        }

        #region Res 与 Model关系

        /// <summary>
        /// 本质：Resource到Model的转换
        /// 请根据request.ResName，提供Res到Model的转换.
        /// </summary>
        protected abstract Task<IEnumerable<TModel>> GetFromRemoteAsync(IApiClient apiClient, ApiRequest request);

        protected abstract Task AddToRemoteAsync(IApiClient apiClient, IEnumerable<TModel> models);

        protected abstract Task UpdateToRemoteAsync(IApiClient apiClient, IEnumerable<PropertyChangePack> changedPacks);

        protected abstract Task DeleteFromRemoteAsync(IApiClient apiClient, IEnumerable<TModel> models);

        #endregion

        #region 获取 - 发生在Syncing之后 - 从服务器上获取整体后，更新整体

        protected async Task<TModel?> GetFirstOrDefaultAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            GetSetMode getMode,
            IfUseLocalData<TModel>? ifUseLocalData = null)
        {
            IEnumerable<TModel> models = await GetAsync(localWhere, remoteRequest, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);

            return models.FirstOrDefault();
        }

        protected async Task<IEnumerable<TModel>> GetAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            GetSetMode getMode,
            IfUseLocalData<TModel>? ifUseLocalData = null)
        {
            _syncManager.WaitUntilNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            StartTrack(locals);

            //如果强制获取本地，则返回本地
            if (getMode == GetSetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{Type}", typeof(TModel).Name);
                return locals;
            }

            return await SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData).ConfigureAwait(false);
        }

        protected async Task<ObservableTask<IEnumerable<TModel>>> GetObservableTaskAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            GetSetMode getMode = GetSetMode.Mixed,
            IfUseLocalData<TModel>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            _syncManager.WaitUntilNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            StartTrack(locals);

            //如果强制获取本地，则返回本地
            if (getMode == GetSetMode.LocalForced)
            {
                return new ObservableTask<IEnumerable<TModel>>(locals, null, onException, continueOnCapturedContext);
            }

            return new ObservableTask<IEnumerable<TModel>>(
                locals,
                () => SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData),
                onException,
                continueOnCapturedContext);
        }

        protected async Task<ObservableTask<TModel?>> GetFirstOrDefaultObservableTaskAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            GetSetMode getMode = GetSetMode.Mixed,
            IfUseLocalData<TModel>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            _syncManager.WaitUntilNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            StartTrack(locals);

            //如果强制获取本地，则返回本地
            if (getMode == GetSetMode.LocalForced)
            {
                return new ObservableTask<TModel?>(locals.FirstOrDefault(), null, onException, continueOnCapturedContext);
            }

            ifUseLocalData ??= DefaultIfUseLocalData;

            return new ObservableTask<TModel?>(
                locals.FirstOrDefault(),
                async () => (await SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false)).FirstOrDefault(),
                onException,
                continueOnCapturedContext);
        }

        private async Task<IEnumerable<TModel>> SyncGetAsync(
            IEnumerable<TModel> localModels,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            GetSetMode getMode,
            IfUseLocalData<TModel> ifUseLocalData)
        {
            //如果不强制远程，并且满足使用本地数据条件
            if (getMode != GetSetMode.RemoteForced && ifUseLocalData(remoteRequest, localModels))
            {
                _logger.LogDebug("本地数据可用，返回本地, Type:{Type}", typeof(TModel).Name);
                return localModels;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!_clientEvents.ServerConnected)
            {
                if (ClientModelSetting.AllowOfflineRead)
                {
                    _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{Type}", typeof(TModel).Name);

                    //StatusManager.OnOfflineDataReaded();

                    return localModels;
                }
                else
                {
                    throw ClientExceptions.NoInternet();
                }
            }

            #region 远程读取，本地更新

            IEnumerable<TModel> remotes = await GetFromRemoteAsync(ApiClient, remoteRequest).ConfigureAwait(false);

            _logger.LogDebug("远程数据获取完毕, Type:{Type}", typeof(TModel).Name);

            foreach (TModel model in remotes)
            {
                //NOTICE:这里是在每次重新上线后的Syncing之后运行的
                //所以，只要覆盖即可

                //TODO: 批量执行
                await Database.AddOrUpdateByIdAsync(model, "", transactionContext).ConfigureAwait(false);
            }

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{Type}", typeof(TModel).Name);

            StartTrack(remotes);

            return remotes;

            #endregion
        }

        /// <summary>
        /// 本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
        /// </summary>
        private bool DefaultIfUseLocalData(ApiRequest request, IEnumerable<TModel> localModels)
        {
            return localModels.Any() && localModels.All(t => TimeUtil.UtcNow - t.LastTime < ClientModelSetting.ExpiryTime);
        }

        #endregion

        #region 更改 - 发生在Syncing之后

        public async Task AddAsync(IList<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));

            //等待同步完，包括处理完冲突
            _syncManager.WaitUntilNotSyncing();

            try
            {
                //Network First
                if (!_clientEvents.ServerConnected)
                {
                    if (ClientModelSetting.AllowOfflineAdd)
                    {
                        //Offline History
                        await _syncManager.RecordOfflineAddAsync(models, transactionContext).ConfigureAwait(false);
                    }
                    else
                    {
                        throw ClientExceptions.NoInternet();
                    }
                }
                else
                {
                    //Remote
                    //TODO: 罗列处理可能的异常：1， 存在重复；
                    await AddToRemoteAsync(ApiClient, models).ConfigureAwait(false);
                }

                //Local
                await Database.AddAsync(models, "", transactionContext).ConfigureAwait(false);

                //Clear Tracks
                ClearPropertyTracks(models);
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.DuplicateKeyEntry)
            {
                //TODO: 测试这个
                //有可能是网络抖动，或者重复请求，所以，忽略就好
            }
        }

        public async Task UpdateAsync(IEnumerable<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));
            _syncManager.WaitUntilNotSyncing();

            IList<PropertyChangePack> changedPacks = models.Select(m => m.GetPropertyChangePack()).ToList();

            try
            {
                //Network First
                if (!_clientEvents.ServerConnected)
                {
                    if (ClientModelSetting.AllowOfflineUpdate)
                    {
                        await _syncManager.RecordOfflineUpdateAsync<TModel>(changedPacks, transactionContext).ConfigureAwait(false);
                    }
                    else
                    {
                        throw ClientExceptions.NoInternet();
                    }
                }
                else
                {
                    //TODO: 处理ErrorCodes.ConcurrencyConflict, 也有可能需要service层，或者Vm层由用户决定； 或者就后来者居上
                    //TODO: 比如有些数据，就是后来者居上，有些数据不行

                    await UpdateToRemoteAsync(ApiClient, changedPacks).ConfigureAwait(false);
                }

                //Local
                await Database.UpdatePropertiesAsync<TModel>(changedPacks, "", transactionContext).ConfigureAwait(false);

                //Clear Tracks
                ClearPropertyTracks(models);
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.ConcurrencyConflict)
            {
                //TODO:处理冲突, 是不是需要区分来自于网络还是本地Batch
            }
        }

        public async Task DeleteAsync(IList<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));
            _syncManager.WaitUntilNotSyncing();

            try
            {
                if (!_clientEvents.ServerConnected)
                {
                    if (ClientModelSetting.AllowOfflineDelete)
                    {
                        await _syncManager.RecordOfflineDeleteAsync(models, transactionContext).ConfigureAwait(false);
                    }
                    else
                    {
                        throw ClientExceptions.NoInternet();
                    }
                }
                else
                {
                    await DeleteFromRemoteAsync(ApiClient, models).ConfigureAwait(false);
                }

                await Database.DeleteAsync(models, "", transactionContext).ConfigureAwait(false);
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.ConcurrencyConflict)
            {
                //TODO:处理冲突, 是不是需要区分来自于网络还是本地Batch
            }
        }

        #endregion

        private static void ClearPropertyTracks(IEnumerable<TModel> models)
        {
            foreach (TModel model in models)
            {
                if (model is IPropertyTrackableObject trackableObject)
                {
                    model.Clear();
                }
            }
        }

        private void StartTrack(IEnumerable<TModel> models)
        {
            foreach (TModel model in models)
            {
                if (model is IPropertyTrackableObject trackableObject)
                {
                    model.StartTrack();
                }
            }
        }
    }
}