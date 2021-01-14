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

namespace HB.FullStack.Mobile.Repos
{
    public abstract class BaseRepo
    {
        /// <summary>
        /// InsureLogined
        /// </summary>
        /// <exception cref="ApiException"></exception>
        protected static void InsureLogined()
        {
            if (!UserPreferences.IsLogined)
            {
                throw new ApiException(ApiErrorCode.NoAuthority);
            }
        }

        /// <summary>
        /// InsureInternet
        /// </summary>
        /// <param name="allowOffline"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        protected static bool InsureInternet(bool allowOffline = false)
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

        /// <summary>
        /// GetSingleAsync
        /// </summary>
        /// <param name="where"></param>
        /// <param name="request"></param>
        /// <param name="transactionContext"></param>
        /// <param name="forced"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        protected async Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> where, ApiRequest<TRes> request, TransactionContext? transactionContext = null, bool forced = false)
        {
            if (NeedLogined)
            {
                InsureLogined();
            }

            if (!forced && LocalDataAvailable(request))
            {
                TEntity? local = await Database.ScalarAsync(where, transactionContext).ConfigureAwait(false);

                if (local != null)
                {
                    return local;
                }
            }

            if (!InsureInternet(AllowOfflineRead))
            {
                //被迫使用离线数据
                NotifyOfflineDataUsed();

                TEntity? local = await Database.ScalarAsync(where, transactionContext).ConfigureAwait(false);

                if (local != null)
                {
                    return local;
                }

                throw new ApiException(ApiErrorCode.ApiNotAvailable);
            }

            //获取全程，更新本地

            TRes? res = await ApiClient.GetSingleAsync(request).ConfigureAwait(false);

            if (res == null)
            {
                return null;
            }

            TEntity remote = ToEntity(res);

            await Database.AddOrUpdateByIdAsync(remote).ConfigureAwait(false); //考虑Fire（）

            return remote;
        }

        

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="where"></param>
        /// <param name="request"></param>
        /// <param name="transactionContext"></param>
        /// <param name="forced"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        protected async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> where, ApiRequest<TRes> request, TransactionContext transactionContext, bool forced = false)
        {
            if (NeedLogined)
            {
                InsureLogined();
            }

            if (!forced && LocalDataAvailable(request))
            {
                IEnumerable<TEntity> locals = await Database.RetrieveAsync(where, null).ConfigureAwait(false);

                if (locals.Any())
                {
                    return locals;
                }
            }

            if (!InsureInternet(AllowOfflineRead))
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

        /// <summary>
        /// AddAsync
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task AddAsync(IEnumerable<TEntity> entities, TransactionContext transactionContext)
        {
            ThrowIf.NotValid(entities, nameof(entities));

            if (InsureInternet(AllowOfflineWrite))
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

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task UpdateAsync(TEntity entity, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotValid(entity, nameof(entity));

            if (InsureInternet(AllowOfflineWrite))
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