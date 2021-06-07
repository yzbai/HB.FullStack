using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.Mobile.Api;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Database;
using Xamarin.Essentials;
using Xamarin.Forms;
using HB.FullStack.Mobile.Base;
using Microsoft;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Mobile.Base
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
                    baseApplication.InitializeTask.Wait();
                }

                _isAppInitTaskFinished = true;
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static void EnsureLogined()
        {
            if (!UserPreferences.IsLogined)
            {
                throw ApiExceptions.NoAuthority();
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static bool EnsureInternet(bool throwIfNot = true)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if (throwIfNot)
                {
                    throw ApiExceptions.NoInternet(cause:"没有联网，且不允许离线");
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

    public delegate bool IfUseLocalData<TEntity, TRes>(ApiRequest<TRes> request, IEnumerable<TEntity> entities) where TEntity : DatabaseEntity, new() where TRes : ApiResource;

    public abstract class BaseRepo<TEntity, TRes> : BaseRepo where TEntity : DatabaseEntity, new() where TRes : ApiResource
    {
        protected IDatabase Database { get;}

        protected IApiClient ApiClient { get; }

        private readonly TimeSpan _localDataExpiryTime;
        private readonly ILogger _logger;

        protected abstract bool AllowOfflineWrite { get; }

        protected abstract bool AllowOfflineRead { get; }

        protected abstract bool NeedLogined { get; }

        protected BaseRepo(ILogger logger, IDatabase database, IApiClient apiClient)
        {
            _logger = logger;
            Database = database;
            ApiClient = apiClient;

            TimeSpan? attributedLocalDataExpiryTime = typeof(TEntity).GetCustomAttribute<LocalDataTimeoutAttribute>()?.ExpiryTime;

            _localDataExpiryTime = attributedLocalDataExpiryTime == null ? Conventions.DefaultLocalDataExpiryTime : attributedLocalDataExpiryTime.Value;
        }

        protected abstract IEnumerable<TEntity> ToEntities(TRes res);

        protected abstract IEnumerable<TRes> ToResources(TEntity entity);

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        protected async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TEntity, TRes>? ifUseLocalData = null)
        {
            if (NeedLogined)
            {
                _logger.LogDebug("检查Logined, Type:{type}", typeof(TEntity).Name);

                EnsureLogined();
            }

            //TODO:尝试引入Cache

            IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
                _logger.LogDebug("本地强制模式，返回, Type:{type}", typeof(TEntity).Name);
                return locals;
            }

            if(ifUseLocalData == null)
            {
                ifUseLocalData = DefaultLocalDataAvaliable;
            }

            return await NetworkBoundGetAsync(locals, request, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);
        }

        protected async Task<ObservableTask<IEnumerable<TEntity>>> GetObservableTaskAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.None,
            IfUseLocalData<TEntity, TRes>? ifUseLocalData = null)
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

            if (ifUseLocalData == null)
            {
                ifUseLocalData = DefaultLocalDataAvaliable;
            }

            return new ObservableTask<IEnumerable<TEntity>>(
                locals,
                () => NetworkBoundGetAsync(locals, request, transactionContext, getMode, ifUseLocalData),
                BaseApplication.ExceptionHandler);
        }

        protected async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext,
            RepoGetMode getMode,
            IfUseLocalData<TEntity, TRes>? ifUseLocalData = null)
        {
            IEnumerable<TEntity> entities = await GetAsync(where, request, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false);

            return entities.FirstOrDefault();
        }

        protected async Task<ObservableTask<TEntity?>> GetFirstOrDefaultObservableTaskAsync(
            Expression<Func<TEntity, bool>> where,
            ApiRequest<TRes> request,
            TransactionContext? transactionContext = null,
            RepoGetMode getMode = RepoGetMode.None,
            IfUseLocalData<TEntity, TRes>? ifUseLocalData = null)
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

            if (ifUseLocalData == null)
            {
                ifUseLocalData = DefaultLocalDataAvaliable;
            }

            return new ObservableTask<TEntity?>(
                locals.FirstOrDefault(),
                async () => (await NetworkBoundGetAsync(locals, request, transactionContext, getMode, ifUseLocalData).ConfigureAwait(false)).FirstOrDefault(),
                BaseApplication.ExceptionHandler);
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        private async Task<IEnumerable<TEntity>> NetworkBoundGetAsync(IEnumerable<TEntity> locals, ApiRequest<TRes> request, TransactionContext? transactionContext, RepoGetMode getMode, IfUseLocalData<TEntity, TRes> ifUseLocalData )
        {
            //如果不强制远程，并且满足使用本地数据条件
            if (getMode != RepoGetMode.RemoteForced && ifUseLocalData(request, locals))
            {
                _logger.LogDebug("本地数据可用，返回本地, Type:{type}", typeof(TEntity).Name);
                return locals;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!EnsureInternet(!AllowOfflineRead))
            {
                _logger.LogDebug("未联网，允许离线读， 使用离线数据, Type:{type}", typeof(TEntity).Name);
                NotifyOfflineDataUsed();

                return locals;
            }

            //获取远程，更新本地
            IEnumerable<TRes> ress = await ApiClient.GetAsync(request).ConfigureAwait(false);
            IEnumerable<TEntity> remotes = ress.SelectMany(res => ToEntities(res)).ToList();

            _logger.LogDebug("远程数据获取完毕, Type:{type}", typeof(TEntity).Name);

            //版本1：如果Id每次都是随机，会造成永远只添加，比如AliyunStsToken，服务器端返回Id = -1，导致每次获取后，Id都不一致
            //所以Id默认为 - 1的实体就不要用Id作为主键了
            foreach (TEntity entity in remotes)
            {
                await Database.AddOrUpdateByIdAsync(entity, transactionContext).ConfigureAwait(false);
            }

            //版本2：先删除locals，然后再添加,由于是假删除，IdBarrier中并没有删除对应关系，导致服务器ID映射的客户端ID重复，这时，不应该用IdGenEntity作为实体，选用Autoincrement，比如AliyunStsToken
            //await Database.BatchDeleteAsync(locals, "", transactionContext).ConfigureAwait(false);
            //await Database.BatchAddAsync(remotes, "", transactionContext).ConfigureAwait(false);

            _logger.LogDebug("重新添加远程数据到本地数据库, Type:{type}", typeof(TEntity).Name);

            return remotes;
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task AddAsync(IEnumerable<TEntity> entities, TransactionContext? transactionContext)
        {
            ThrowIf.NotValid(entities, nameof(entities));

            if (EnsureInternet(!AllowOfflineWrite))
            {
                //Remote
                AddRequest<TRes> addRequest = new AddRequest<TRes>(entities.SelectMany(k => ToResources(k)).ToList());

                await ApiClient.AddAsync(addRequest).ConfigureAwait(false);

                //Local
                await Database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 脱网下操作
                throw new NotSupportedException();
            }
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task UpdateAsync(TEntity entity, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotValid(entity, nameof(entity));

            if (EnsureInternet(!AllowOfflineWrite))
            {
                UpdateRequest<TRes> updateRequest = new UpdateRequest<TRes>(ToResources(entity));

                await ApiClient.UpdateAsync(updateRequest).ConfigureAwait(false);

                await Database.UpdateAsync(entity, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //TODO: 脱网下操作
                throw new NotSupportedException();
            }
        }

        private bool DefaultLocalDataAvaliable(ApiRequest<TRes> request, IEnumerable<TEntity> locals)
        {
            //本地数据不为空且不过期，或者，本地数据为空但最近刚请求过，返回本地
            return
                (locals.Any() && !IsLocalDataExpired(locals, _localDataExpiryTime))
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

        private static bool IsRequestRecently(ApiRequest<TRes> apiRequest)
        {
            TimeSpan expiryTime = apiRequest.GetRateLimit() ?? Conventions.DefaultApiRequestRateLimit;

            return !RequestLocker.NoWaitLock(
                "request",
                apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture),
                expiryTime);
        }

        private static void NotifyOfflineDataUsed()
        {
            BaseApplication.Current.OnOfflineDataUsed();
        }
    }
}