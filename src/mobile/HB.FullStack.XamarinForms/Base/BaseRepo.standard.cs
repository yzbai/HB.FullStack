using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Database;
using HB.FullStack.Database.Def;
using Xamarin.Essentials;
using Xamarin.Forms;
using HB.FullStack.XamarinForms.Base;
using Microsoft;
using System.Diagnostics.CodeAnalysis;

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
                throw new ApiException(ApiErrorCode.NoAuthority);
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static bool EnsureInternet(bool throwIfNot = true)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if (throwIfNot)
                {
                    throw new ApiException(ApiErrorCode.ApiNotAvailable);
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
                throw new ApiException(HB.FullStack.Common.Api.ApiErrorCode.NullReturn, $"Parameter: {entityName}");
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
        protected readonly IDatabase _database;

        protected readonly IApiClient _apiClient;

        private readonly TimeSpan _localDataExpiryTime;

        protected abstract bool AllowOfflineWrite { get; }

        protected abstract bool AllowOfflineRead { get; }

        protected abstract bool NeedLogined { get; }

        protected BaseRepo(IDatabase database, IApiClient apiClient)
        {
            _database = database;
            _apiClient = apiClient;

            TimeSpan? attributedLocalDataExpiryTime = typeof(TEntity).GetCustomAttribute<LocalDataTimeoutAttribute>()?.ExpiryTime;

            _localDataExpiryTime = attributedLocalDataExpiryTime == null ? Consts.DefaultLocalDataExpiryTime : attributedLocalDataExpiryTime.Value;
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
                EnsureLogined();
            }

            //TODO:尝试引入Cache

            IEnumerable<TEntity> locals = await _database.RetrieveAsync(where, null).ConfigureAwait(false);

            //如果强制获取本地，则返回本地
            if (getMode == RepoGetMode.LocalForced)
            {
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

            IEnumerable<TEntity> locals = await _database.RetrieveAsync(where, null).ConfigureAwait(false);

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

            IEnumerable<TEntity> locals = await _database.RetrieveAsync(where, null).ConfigureAwait(false);

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
                return locals;
            }

            //如果没有联网，但允许离线读，被迫使用离线数据
            if (!EnsureInternet(!AllowOfflineRead))
            {
                NotifyOfflineDataUsed();

                return locals;
            }

            //获取远程，更新本地
            IEnumerable<TRes> ress = await _apiClient.GetAsync(request).ConfigureAwait(false);
            IEnumerable<TEntity> remotes = ress.SelectMany(res => ToEntities(res)).ToList();

            //版本1：如果Id每次都是随机，会造成永远只添加，比如AliyunStsToken，服务器端返回Id=-1，导致每次获取后，Id都不一致
            //foreach (TEntity entity in remotes)
            //{
            //    await _database.AddOrUpdateByIdAsync(entity, transactionContext).ConfigureAwait(false);
            //}

            //版本2：先删除locals，然后再添加
            await _database.BatchDeleteAsync(locals, "", transactionContext).ConfigureAwait(false);
            await _database.BatchAddAsync(remotes, "", transactionContext).ConfigureAwait(false);

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

                await _apiClient.AddAsync(addRequest).ConfigureAwait(false);

                //Local
                await _database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);
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

                await _apiClient.UpdateAsync(updateRequest).ConfigureAwait(false);

                await _database.UpdateAsync(entity, "", transactionContext).ConfigureAwait(false);
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
            TimeSpan expiryTime = apiRequest.GetRateLimit() ?? Consts.DefaultApiRequestRateLimit;

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