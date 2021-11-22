using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Database;
using Xamarin.Essentials;
using Xamarin.Forms;
using Microsoft;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using HB.FullStack.Database.Entities;
using Microsoft.VisualStudio.Threading;
using HB.FullStack.Common.ApiClient;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseRepo
    {
        private static bool _isAppInitTaskFinished;

        protected static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

        protected static void CheckAppInitIsFinished()
        {
            if (!_isAppInitTaskFinished)
            {
                if (Application.Current is BaseApplication baseApplication)
                {
                    JoinableTaskUtil.JoinableTaskFactory.Run(async () => await baseApplication.InitializeTask.ConfigureAwait(false));
                    //baseApplication.InitializeTask.Wait();
                }

                _isAppInitTaskFinished = true;
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static void EnsureLogined()
        {
            if (!UserPreferences.IsLogined)
            {
                throw MobileExceptions.NotLogined();
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static bool EnsureInternet(bool throwIfNot = true)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if (throwIfNot)
                {
                    throw MobileExceptions.NoInternet(cause: "没有联网，且不允许离线");
                }

                return false;
            }

            return true;
        }

        /// <exception cref="ApiException"></exception>
        protected static void EnsureApiNotReturnNull([ValidatedNotNull][NotNull] object? obj, string entityName)
        {
            if (obj == null)
            {
                throw ApiExceptions.ServerNullReturn(parameter: entityName);
            }
        }

        protected BaseRepo()
        {
            CheckAppInitIsFinished();
        }
    }

    public delegate bool WhetherUseLocalData<TEntity>(ApiRequest request, IEnumerable<TEntity> entities) where TEntity : DatabaseEntity, new();

    public abstract class BaseRepo<TEntity, TRes> : BaseRepo where TEntity : DatabaseEntity, new() where TRes : ApiResource2
    {
        private readonly ILogger _logger;
        protected IDatabase Database { get; }

        protected IApiClient ApiClient { get; }

        protected TimeSpan LocalDataExpiryTime { get; set; }

        protected bool AllowOfflineWrite { get; set; }

        protected bool AllowOfflineRead { get; set; }

        protected bool NeedLogined { get; set; }

        protected BaseRepo(ILogger logger, IDatabase database, IApiClient apiClient)
        {
            _logger = logger;
            Database = database;
            ApiClient = apiClient;

            LocalDataAttribute? localDataAttribute = typeof(TEntity).GetCustomAttribute<LocalDataAttribute>(true);

            if (localDataAttribute == null)
            {
                LocalDataExpiryTime = Conventions.DefaultLocalDataExpiryTime;
                NeedLogined = true;
                AllowOfflineRead = false;
                AllowOfflineWrite = false;
            }
            else
            {
                LocalDataExpiryTime = TimeSpan.FromSeconds(localDataAttribute.ExpirySeconds);
                NeedLogined = localDataAttribute.NeedLogined;
                AllowOfflineRead = localDataAttribute.AllowOfflineRead;
                AllowOfflineWrite = localDataAttribute.AllowOfflineWrite;
            }
        }

        protected abstract TEntity ToEntity(TRes res);

        protected abstract TRes ToResource(TEntity entity);

        #region 查询

        protected async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest request,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            WhetherUseLocalData<TEntity>? whetherUseLocalData = null)
        {
            if (NeedLogined)
            {
                _logger.LogDebug("检查Logined, Type:{type}", typeof(TEntity).Name);

                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{type}", typeof(TEntity).Name);
                return locals;
            }

            return await SyncGetAsync(locals, request, transactionContext, getMode, whetherUseLocalData ?? DefaultWhetherUseLocalData).ConfigureAwait(false);
        }

        /// <summary>
        /// 先返回本地初始值，再更新为服务器值
        /// </summary>
        protected async Task<ObservableTask<IEnumerable<TEntity>>> GetObservableTaskAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.None,
            WhetherUseLocalData<TEntity>? whetherUseLocalData = null)
        {
            if (NeedLogined)
            {
                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<IEnumerable<TEntity>>(locals, null, BaseApplication.ExceptionHandler);
            }

            return new ObservableTask<IEnumerable<TEntity>>(
                locals,
                () => SyncGetAsync(locals, request, transactionContext, getMode, whetherUseLocalData ?? DefaultWhetherUseLocalData),
                BaseApplication.ExceptionHandler);
        }

        protected async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest request,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            WhetherUseLocalData<TEntity>? ifUseLocalData = null)
        {
            IEnumerable<TEntity> entities = await GetAsync(where, request, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);

            return entities.FirstOrDefault();
        }

        protected async Task<ObservableTask<TEntity?>> GetFirstOrDefaultObservableTaskAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.None,
            WhetherUseLocalData<TEntity>? whetherUseLocalData = null)
        {
            if (NeedLogined)
            {
                EnsureLogined();
            }

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                return new ObservableTask<TEntity?>(locals.FirstOrDefault(), null, BaseApplication.ExceptionHandler);
            }

            if (whetherUseLocalData == null)
            {
                whetherUseLocalData = DefaultWhetherUseLocalData;
            }

            return new ObservableTask<TEntity?>(
                locals.FirstOrDefault(),
                async () => (await SyncGetAsync(locals, request, transactionContext, getMode, whetherUseLocalData).ConfigureAwait(false)).FirstOrDefault(),
                BaseApplication.ExceptionHandler);
        }

        private async Task<IEnumerable<TEntity>> SyncGetAsync(
            IEnumerable<TEntity> locals,
            ApiRequest request,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            WhetherUseLocalData<TEntity> whetherUseLocalData)
        {
            //如果不强制远程，并且满足使用本地数据条件
            if (getMode != RepoGetMode.RemoteForced && whetherUseLocalData(request, locals))
            {
                _logger.LogDebug("本地数据可用，返回本地, Type:{type}", typeof(TEntity).Name);
                return locals;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!EnsureInternet(!AllowOfflineRead))
            {
                _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{type}", typeof(TEntity).Name);

                OnOfflineDataRead();

                return locals;
            }

            //获取远程
            IEnumerable<TRes>? ress = await ApiClient.GetAsync<IEnumerable<TRes>>(request).ConfigureAwait(false);

            IEnumerable<TEntity> remotes = ress!.Select(r => ToEntity(r)).ToList();

            _logger.LogDebug("远程数据获取完毕, Type:{type}", typeof(TEntity).Name);

            //检查同步. 比如：离线创建的数据，现在联线，本地数据反而是新的。
            //多客户端：version相同，但lastuser不同。根据时间合并

            //情况：
            //1，（本地离线产生新数据）同一id数据，lastuser相同，local version 大于 remote version，使用本地更新远程
            //2，（多客户端）同一id数据，local version 等于 remote version，lastuser不同，按lasttime判断使用谁，如果local lasttime更大，使用本地更新远程，否则远程覆盖本地
            //3，同一id数据，local version 小于 remote version，覆盖本地

            //Case
            //1，第一个客户端，离线，疯狂update一条数据，将version变很大，然后第二个客户端在线，过了很久，update了同一条数据。现在第一个客户端在线，get这条数据

            foreach (TEntity entity in remotes)
            {
                await Database.AddOrUpdateByIdAsync(entity, "", transactionContext).ConfigureAwait(false);
            }

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{type}", typeof(TEntity).Name);

            return remotes;
        }

        #endregion

        #region 更改

        public async Task AddAsync(IEnumerable<TEntity> entities, TransactionContext? transactionContext)
        {
            ThrowIf.NotValid(entities, nameof(entities));

            if (EnsureInternet(!AllowOfflineWrite))
            {
                //Remote
                AddRequest<TRes> addRequest = new AddRequest<TRes>(entities.Select(k => ToResource(k)).ToList());

                await ApiClient.SendAsync(addRequest).ConfigureAwait(false);

                //Local
                await Database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 脱网下操作
                throw new NotSupportedException();
            }
        }

        public async Task UpdateAsync(TEntity entity, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotValid(entity, nameof(entity));

            if (EnsureInternet(!AllowOfflineWrite))
            {
                UpdateRequest<TRes> updateRequest = new UpdateRequest<TRes>(ToResource(entity));

                await ApiClient.SendAsync(updateRequest).ConfigureAwait(false);

                await Database.UpdateAsync(entity, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 脱网下操作
                throw new NotSupportedException();
            }
        }

        #endregion

        /// <summary>
        /// 本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
        /// </summary>
        /// <param name="request"></param>
        /// <param name="locals"></param>
        /// <returns></returns>
        private bool DefaultWhetherUseLocalData(ApiRequest request, IEnumerable<TEntity> locals)
        {
            return
                (locals.Any() && !IsLocalDataExpired(locals, LocalDataExpiryTime))
                ||
                (!locals.Any() && IsRequestRecently(request));
        }

        private static bool IsLocalDataExpired(IEnumerable<TEntity> entities, TimeSpan expiryTimeSpan)
        {
            DateTimeOffset now = TimeUtil.UtcNow;

            foreach (TEntity entity in entities)
            {
                if (now - entity.LastTime > expiryTimeSpan)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRequestRecently(ApiRequest apiRequest)
        {
            TimeSpan expiryTime = apiRequest.RateLimit ?? Conventions.DefaultApiRequestRateLimit;

            return !RequestLocker.NoWaitLock(
                "request",
                apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture),
                expiryTime);
        }

        private static void OnOfflineDataRead()
        {
            BaseApplication.Current.OnOfflineDataUsed();
        }
    }
}