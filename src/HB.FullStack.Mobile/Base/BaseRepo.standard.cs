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
using HB.FullStack.Database.Def;
using Xamarin.Essentials;
using Xamarin.Forms;
using HB.FullStack.Mobile.Base;
using Microsoft;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Mobile.Repos
{
    public abstract class BaseRepo
    {
        /// <exception cref="ApiException"></exception>
        protected static void EnsureLogined()
        {
            if (!UserPreferences.IsLogined)
            {
                throw new ApiException(ApiErrorCode.NoAuthority);
            }
        }

        /// <exception cref="ApiException"></exception>
        protected static bool EnsureInternet(bool allowOffline = false)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if (!allowOffline)
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
    }

    public abstract class BaseRepo<TEntity, TRes> : BaseRepo where TEntity : DatabaseEntity, new() where TRes : ApiResource
    {
        private static bool _isDatabaseInitTaskNotWaitedYet = true;

        protected static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

        protected IDatabase Database { get; }

        protected IApiClient ApiClient { get; }

        private readonly TimeSpan? _localDataExpiryTime;

        protected BaseRepo(IDatabase database, IApiClient apiClient)
        {
            Database = database;
            ApiClient = apiClient;

            if (_isDatabaseInitTaskNotWaitedYet)
            {
                if (Application.Current is BaseApplication baseApplication)
                {
                    baseApplication.InitializeTask.Wait();
                }

                _isDatabaseInitTaskNotWaitedYet = false;
            }

            _localDataExpiryTime = typeof(TEntity).GetCustomAttribute<LocalDataTimeoutAttribute>()?.ExpiryTime;
        }

        protected abstract TEntity ToEntity(TRes res);

        protected abstract TRes ToResource(TEntity entity);

        protected abstract bool AllowOfflineWrite { get; }

        protected abstract bool AllowOfflineRead { get; }

        protected abstract bool NeedLogined { get; }

        protected bool LocalDataAvailable(ApiRequest<TRes> apiRequest)
        {
            if (_localDataExpiryTime.HasValue)
            {
                return RequestLocker.NoWaitLock(apiRequest.GetType().FullName!, apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture), _localDataExpiryTime.Value);
            }

            return false;
        }

        protected static void TimeoutLocalData(ApiRequest<TRes> apiRequest)
        {
            RequestLocker.UnLock(apiRequest.GetType().FullName!, apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        protected async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> where, ApiRequest<TRes> request, TransactionContext? transactionContext, bool forced = false)
        {
            if (NeedLogined)
            {
                EnsureLogined();
            }

            if (!forced && LocalDataAvailable(request))
            {
                IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

                if (locals.Any())
                {
                    return locals;
                }
            }

            if (!EnsureInternet(AllowOfflineRead))
            {
                //被迫使用离线数据
                NotifyOfflineDataUsed();

                IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

                if (locals.Any())
                {
                    return locals;
                }

                throw new ApiException(ApiErrorCode.ApiNotAvailable);
            }

            //获取远程，更新本地

            IEnumerable<TRes> ress = await ApiClient.GetAsync(request).ConfigureAwait(false);

            IEnumerable<TEntity> remotes = ress.Select(res => ToEntity(res));

            await Database.BatchAddOrUpdateByIdAsync(remotes, transactionContext).ConfigureAwait(false); //考虑Fire（）

            return remotes;
        }

        protected async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, ApiRequest<TRes> request, TransactionContext? transactionContext, bool forced = false)
        {
            IEnumerable<TEntity> entities = await GetAsync(where, request, transactionContext, forced).ConfigureAwait(false);

            return entities.FirstOrDefault();
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task AddAsync(IEnumerable<TEntity> entities, TransactionContext? transactionContext)
        {
            ThrowIf.NotValid(entities, nameof(entities));

            if (EnsureInternet(AllowOfflineWrite))
            {
                //Remote
                AddRequest<TRes> addRequest = new AddRequest<TRes>(entities.Select(k => ToResource(k)));

                await ApiClient.AddAsync(addRequest).ConfigureAwait(false);

                //Local
                await Database.BatchAddAsync(entities, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //脱网下操作
                throw new NotSupportedException();
            }
        }

        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task UpdateAsync(TEntity entity, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotValid(entity, nameof(entity));

            if (EnsureInternet(AllowOfflineWrite))
            {
                UpdateRequest<TRes> updateRequest = new UpdateRequest<TRes>(ToResource(entity));

                await ApiClient.UpdateAsync(updateRequest).ConfigureAwait(false);

                await Database.UpdateAsync(entity, "", transactionContext).ConfigureAwait(false);
            }
            else
            {
                //脱网下操作
                throw new NotSupportedException();
            }
        }

        private static void NotifyOfflineDataUsed()
        {
            if (Application.Current is BaseApplication baseApplication)
            {
                baseApplication.OnOfflineDataUsed();
            }
        }
    }
}