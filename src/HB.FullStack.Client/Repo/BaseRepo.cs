using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.Network;
using HB.FullStack.Client.Offline;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Client
{
    public delegate bool IfUseLocalData<TModel>(ApiRequest request, IEnumerable<TModel> models) where TModel : DbModel;

    public abstract class BaseRepo
    {
        protected IPreferenceProvider PreferenceProvider { get; }
        protected StatusManager StatusManager { get; }

        protected IApiClient ApiClient { get; }

        protected void EnsureLogined()
        {
            if (!PreferenceProvider.IsLogined())
            {
                throw ClientExceptions.NotLogined();
            }
        }

        //protected bool IsInternetConnected(bool throwIfNot = true)
        //{
        //    bool isInternetConnected = StatusManager.IsInternet();

        //    if (throwIfNot && !isInternetConnected)
        //    {
        //        throw ClientExceptions.NoInternet("没有联网，且不允许离线");
        //    }

        //    return isInternetConnected;
        //}

        protected void EnsureInternetConnected()
        {
            if (!StatusManager.IsInternet())
            {
                throw ClientExceptions.NoInternet("没有联网");
            }
        }

        protected void EnsureNotSyncing()
        {
            if (StatusManager.NeedSyncAfterReconnected)
            {
                throw ClientExceptions.OperationInvalidCauseofSyncingAfterReconnected();
            }
        }

        protected static void EnsureApiNotReturnNull([ValidatedNotNull][NotNull] object? obj, string modelName)
        {
            if (obj == null)
            {
                throw CommonExceptions.ServerNullReturn(parameter: modelName);
            }
        }

        protected BaseRepo(IApiClient apiClient, IPreferenceProvider userPreferenceProvider, StatusManager connectivityManager)
        {
            ApiClient = apiClient;
            PreferenceProvider = userPreferenceProvider;
            StatusManager = connectivityManager;
        }
    }

    public abstract class BaseRepo<TModel> : BaseRepo where TModel : ClientDbModel, new()
    {
        private readonly ILogger _logger;
        private readonly IHistoryManager _historyManager;
        private readonly DbModelDef _modelDef = null!;

        protected IDatabase Database { get; }

        protected ClientModelDef ClientModelDef { get; set; }

        protected BaseRepo(
            ILogger logger,
            IDatabase database,
            IApiClient apiClient,
            IHistoryManager historyManager,
            IPreferenceProvider userPreferenceProvider,
            StatusManager connectivityManager) : base(apiClient, userPreferenceProvider, connectivityManager)
        {
            _logger = logger;
            _modelDef = database.ModelDefFactory.GetDef<TModel>()!;

            ClientModelDef = ClientModelDefFactory.Get<TModel>() ?? CreateDefaultClientModelDef();
            Database = database;
            _historyManager = historyManager;

            //NOTICE: Move this to options?
            static ClientModelDef CreateDefaultClientModelDef()
            {
                return new ClientModelDef
                {
                    ExpiryTime = TimeSpan.FromSeconds(ClientModelAttribute.DefaultExpirySeconds),
                    AllowOfflineRead = true,
                    AllowOfflineWrite = false
                };
            }
        }

        #region Res 与 Model关系
        /// <summary>
        /// 本质：Resource到Model的转换
        /// 请根据request.ResName，提供Res到Model的转换.
        /// </summary>
        protected abstract Task<IEnumerable<TModel>> GetFromRemoteAsync(IApiClient apiClient, ApiRequest request);

        protected abstract Task AddToRemoteAsync(IApiClient apiClient, IEnumerable<TModel> models);

        protected abstract Task UpdateToRemoteAsync(IApiClient apiClient, IEnumerable<TModel> models);

        protected abstract Task DeleteFromRemoteAsync(IApiClient apiClient, IEnumerable<TModel> models);

        #endregion

        #region 查询 - 发生在Syncing之后

        protected async Task<IEnumerable<TModel>> GetAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TModel>? ifUseLocalData = null)
        {
            EnsureNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{Type}", typeof(TModel).Name);
                return locals;
            }

            return await SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData).ConfigureAwait(false);
        }

        /// <summary>
        /// 先返回本地初始值，再更新为服务器值
        /// </summary>
        protected async Task<ObservableTask<IEnumerable<TModel>>> GetObservableTaskAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.Mixed,
            IfUseLocalData<TModel>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            EnsureNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<IEnumerable<TModel>>(locals, null, onException, continueOnCapturedContext);
            }

            return new ObservableTask<IEnumerable<TModel>>(
                locals,
                () => SyncGetAsync(locals, remoteRequest, transactionContext, getMode, ifUseLocalData ?? DefaultIfUseLocalData),
                onException,
                continueOnCapturedContext);
        }

        protected async Task<TModel?> GetFirstOrDefaultAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TModel>? ifUseLocalData = null)
        {
            EnsureNotSyncing();

            IEnumerable<TModel> models = await GetAsync(localWhere, remoteRequest, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);

            return models.FirstOrDefault();
        }

        protected async Task<ObservableTask<TModel?>> GetFirstOrDefaultObservableTaskAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.Mixed,
            IfUseLocalData<TModel>? ifUseLocalData = null,
            Action<Exception>? onException = null,
            bool continueOnCapturedContext = false)
        {
            EnsureNotSyncing();

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<TModel?>(locals.FirstOrDefault(), null, onException, continueOnCapturedContext);
            }

            if (ifUseLocalData == null)
            {
                ifUseLocalData = DefaultIfUseLocalData;
            }

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
            RepoGetMode getMode,
            IfUseLocalData<TModel> ifUseLocalData)
        {
            //如果不强制远程，并且满足使用本地数据条件
            if (getMode != RepoGetMode.RemoteForced && ifUseLocalData(remoteRequest, localModels))
            {
                _logger.LogDebug("本地数据可用，返回本地, Type:{Type}", typeof(TModel).Name);
                return localModels;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!StatusManager.IsInternet())
            {
                if (ClientModelDef.AllowOfflineRead)
                {
                    _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{Type}", typeof(TModel).Name);

                    StatusManager.OnOfflineDataReaded();

                    return localModels;
                }
                else
                {
                    throw ClientExceptions.NoInternet("没有联网，且不允许离线");
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
                await Database.SetByIdAsync(model, transactionContext).ConfigureAwait(false);
            }

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{Type}", typeof(TModel).Name);

            return remotes;

            #endregion
        }

        /// <summary>
        /// 本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
        /// </summary>
        private bool DefaultIfUseLocalData(ApiRequest request, IEnumerable<TModel> localModels)
        {
            return localModels.Any() && localModels.All(t => TimeUtil.UtcNow - t.UpdatedTime < ClientModelDef.ExpiryTime);
        }

        #endregion

        #region 更改 - 发生在Syncing之后

        public async Task AddAsync(IEnumerable<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));
            EnsureNotSyncing();

            try
            {
                //正常
                if (StatusManager.IsInternet())
                {
                    //Remote
                    //TODO: 罗列处理可能的异常：1， 存在重复；
                    await AddToRemoteAsync(ApiClient, models).ConfigureAwait(false);

                    //Local
                    await Database.BatchAddAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                //离线写
                else if (ClientModelDef.AllowOfflineWrite)
                {
                    //Offline History
                    await _historyManager.RecordOfflineHistryAsync(models, HistoryType.Add, transactionContext).ConfigureAwait(false);

                    //Local
                    await Database.BatchAddAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                else
                {
                    throw ClientExceptions.NoInternet("没有联网，且不允许离线");
                }
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.DuplicateKeyEntry)
            {
                //TODO: 测试这个

            }
        }

        public async Task UpdateAsync(IEnumerable<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));
            EnsureNotSyncing();

            try
            {
                if (StatusManager.IsInternet())
                {
                    await UpdateToRemoteAsync(ApiClient, models).ConfigureAwait(false);

                    await Database.BatchUpdateAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                else if (ClientModelDef.AllowOfflineWrite)
                {
                    await _historyManager.RecordOfflineHistryAsync(models, HistoryType.Update, transactionContext).ConfigureAwait(false);

                    await Database.BatchUpdateAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                else
                {
                    throw ClientExceptions.NoInternet("没有联网，且不允许离线");
                }
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.ConcurrencyConflict)
            {
                //TODO:处理冲突, 是不是需要区分来自于网络还是本地Batch
            }
        }

        public async Task DeleteAsync(IEnumerable<TModel> models, TransactionContext transactionContext)
        {
            ThrowIf.NullOrEmpty(models, nameof(models));
            ThrowIf.NotValid(models, nameof(models));
            EnsureNotSyncing();

            try
            {
                if (StatusManager.IsInternet())
                {
                    await DeleteFromRemoteAsync(ApiClient, models).ConfigureAwait(false);

                    await Database.BatchDeleteAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                else if (ClientModelDef.AllowOfflineWrite)
                {
                    await _historyManager.RecordOfflineHistryAsync(models, HistoryType.Delete, transactionContext).ConfigureAwait(false);

                    await Database.BatchDeleteAsync(models, "", transactionContext).ConfigureAwait(false);
                }
                else
                {
                    throw ClientExceptions.NoInternet("没有联网，且不允许离线");
                }
            }
            catch (ErrorCodeException ex) when (ex.ErrorCode == ErrorCodes.ConcurrencyConflict)
            {
                //TODO:处理冲突, 是不是需要区分来自于网络还是本地Batch
            }
        }

        #endregion
    }
}