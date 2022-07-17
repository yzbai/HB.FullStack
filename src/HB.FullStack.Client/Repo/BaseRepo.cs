using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.Network;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Database;
using HB.FullStack.Database.DBModels;
//using Xamarin.Essentials;
//using Xamarin.Forms;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Client
{
    public delegate bool IfUseLocalData<TModel>(ApiRequest request, IEnumerable<TModel> models) where TModel : DBModel, new();

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

        protected static void EnsureApiNotReturnNull([ValidatedNotNull][NotNull] object? obj, string modelName)
        {
            if (obj == null)
            {
                throw ApiExceptions.ServerNullReturn(parameter: modelName);
            }
        }

        protected BaseRepo(IApiClient apiClient, IPreferenceProvider userPreferenceProvider, ConnectivityManager connectivityManager)
        {
            ApiClient = apiClient;
            PreferenceProvider = userPreferenceProvider;
            ConnectivityManager = connectivityManager;
        }
    }

    public abstract class BaseRepo<TModel/*, TRes*/> : BaseRepo where TModel : DBModel//, new() where TRes : ApiResource
    {
        private readonly ILogger _logger;
        private readonly DBModelDef _modelDef = null!;

        protected IDatabase Database { get; }

        protected ClientModelDef ClientModelDef { get; set; }

        protected BaseRepo(
            ILogger logger,
            IDatabase database,
            IApiClient apiClient,
            IPreferenceProvider userPreferenceProvider,
            ConnectivityManager connectivityManager) : base(apiClient, userPreferenceProvider, connectivityManager)
        {
            _logger = logger;
            _modelDef = database.ModelDefFactory.GetDef<TModel>()!;

            Database = database;

            ClientModelDef? clientModelDef = ClientModelDefFactory.Get<TModel>();

            if (clientModelDef == null)
            {
                clientModelDef = CreateDefaultClientModelDef();
            }

            ClientModelDef = clientModelDef;

            //NOTICE: Move this to options?
            static ClientModelDef CreateDefaultClientModelDef()
            {
                return new ClientModelDef
                {
                    ExpiryTime = TimeSpan.FromSeconds(ClientModelAttribute.DefaultExpirySeconds),
                    NeedLogined = true,
                    AllowOfflineRead = true,
                    AllowOfflineWrite = false
                };
            }
        }

        //protected abstract TModel ToModel(TRes res);

        //protected abstract TRes ToResource(TModel model);

        #region 查询

        protected async Task<IEnumerable<TModel>> GetAsync(
            Expression<Func<TModel, bool>> localWhere,
            ApiRequest remoteRequest,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TModel>? ifUseLocalData = null)
        {
            EnsureNotSyncing();

            //TODO: await Syncing();

            //TODO: 是否应该由ClientModel来决定需要login？或者由业务决定
            if (ClientModelDef.NeedLogined)
            {
                _logger.LogDebug("检查Logined, Type:{Type}", typeof(TModel).Name);

                EnsureLogined();
            }

            IEnumerable<TModel> locals = await Database.RetrieveAsync(localWhere, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{type}", typeof(TModel).Name);
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

            if (ClientModelDef.NeedLogined)
            {
                EnsureLogined();
            }

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

            if (ClientModelDef.NeedLogined)
            {
                EnsureLogined();
            }

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
            if (!IsInternetConnected(!ClientModelDef.AllowOfflineRead))
            {
                _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{Type}", typeof(TModel).Name);

                ConnectivityManager.OnOfflineDataReaded();

                return localModels;
            }

            //获取远程
            IEnumerable<TRes>? ress = await ApiClient.GetAsync<IEnumerable<TRes>>(remoteRequest).ConfigureAwait(false);

            IEnumerable<TModel> remotes = ress!.Select(r => ToModel(r)).ToList();

            _logger.LogDebug("远程数据获取完毕, Type:{Type}", typeof(TModel).Name);

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

            //TODO: 这里不管不顾，直接用远程覆盖，是否要比较一下localModels和remotes. Version的大小
            //如果本地Version大
            //如果本地Version小
            foreach (TModel model in remotes)
            {
                //TODO: Reset一遍，覆盖本地？
                await Database.SetByIdAsync(model, transactionContext).ConfigureAwait(false);
            }

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{Type}", typeof(TModel).Name);

            return remotes;
        }

        /// <summary>
        /// 本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
        /// </summary>
        private bool DefaultIfUseLocalData(ApiRequest request, IEnumerable<TModel> localModels)
        {
            return localModels.Any() && localModels.All(t => TimeUtil.UtcNow - t.LastTime < ClientModelDef.ExpiryTime);
        }

        #endregion

        #region 更改

        public async Task AddAsync(IEnumerable<TModel> models, TransactionContext transactionContext)
        {
            EnsureNotSyncing();

            ThrowIf.NotValid(models, nameof(models));

            if (!models.Any())
            {
                return;
            }

            if (IsInternetConnected(!ClientModelDef.AllowOfflineWrite))
            {
                //TODO: 这里的ApiRequestAuth从哪里获得?
                //Remote
                AddRequest<TRes> addRequest = new AddRequest<TRes>(models.Select(k => ToResource(k)).ToList(), ApiRequestAuth.JWT, null);

                await ApiClient.SendAsync(addRequest).ConfigureAwait(false);

                //Local
                await Database.BatchAddAsync(models, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                throw new NotImplementedException();
                //允许脱网下写操作

                //Local
                //await Database.BatchAddAsync(models, "", transactionContext).ConfigureAwait(false);

                //Record History
                //await Database.BatchAddAsync(GetOfflineHistories(models, DbOperation.Add), "", transactionContext).ConfigureAwait(false);
            }
        }

        public async Task UpdateAsync(TModel model, TransactionContext transactionContext)
        {
            EnsureNotSyncing();

            ThrowIf.NotValid(model, nameof(model));

            if (IsInternetConnected(!ClientModelDef.AllowOfflineWrite))
            {
                //TODO: 这里的ApiRequestAuth从哪里获得?
                UpdateRequest<TRes> updateRequest = new UpdateRequest<TRes>(ToResource(model), ApiRequestAuth.JWT, null);

                //如果Version不对，会返回NotFount ErrorCode
                await ApiClient.SendAsync(updateRequest).ConfigureAwait(false);

                await Database.UpdateAsync(model, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 允许脱网下写操作
                throw new NotImplementedException();
            }
        }

        private List<OfflineHistory> GetOfflineHistories(IEnumerable<TModel> models, DbOperation dbOperation)
        {
            List<OfflineHistory> histories = new List<OfflineHistory>(models.Count());

            if (_modelDef.IsIdLong)
            {
                foreach (TModel model in models)
                {
                    OfflineHistory history = new OfflineHistory
                    {
                        ModelId = (model as TimestampLongIdDBModel)!.Id.ToString(CultureInfo.InvariantCulture),
                        ModelFullName = _modelDef.ModelFullName,
                        Operation = dbOperation,
                        OperationTime = model.LastTime,
                        Handled = false
                    };

                    histories.Add(history);
                }
            }
            else if (_modelDef.IsIdGuid)
            {
                foreach (TModel model in models)
                {
                    OfflineHistory history = new OfflineHistory
                    {
                        ModelId = (model as TimestampGuidDBModel)!.Id.ToString(),
                        ModelFullName = _modelDef.ModelFullName,
                        Operation = dbOperation,
                        OperationTime = model.LastTime,
                        Handled = false
                    };

                    histories.Add(history);
                }
            }
            else
            {
                throw ClientExceptions.UnSupportedModelType(_modelDef.ModelFullName);
            }

            return histories;
        }

        #endregion
    }
}