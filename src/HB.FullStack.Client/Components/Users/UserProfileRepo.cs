/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Base;
using HB.FullStack.Client.Components.Sync;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.Components.Users
{
    public class UserProfileRepo : BaseRepo<UserProfile>
    {
        public UserProfileRepo(
            ILogger<UserProfileRepo> logger,
            IClientModelSettingFactory clientModelSettingFactory,
            IDatabase database,
            IApiClient apiClient,
            ISyncManager syncManager,
            IClientEvents clientEvents,
            ITokenPreferences clientPreferences) : base(logger, clientModelSettingFactory, database, apiClient, syncManager, clientEvents, clientPreferences)
        { }

        internal Task<UserProfile?> GetByUserIdAsync(Guid userId, TransactionContext? transactionContext, GetSetMode getMode = GetSetMode.Mixed, IfUseLocalData<UserProfile>? ifUseLocalData = null)
        {
            return GetFirstOrDefaultAsync(
                localWhere: userProfile => userProfile.UserId == userId,
                remoteRequest: new UserProfileResGetByUserIdRequest(userId),
                transactionContext: transactionContext,
                getMode: getMode,
                ifUseLocalData: ifUseLocalData);
        }

        internal Task<ObservableTask<UserProfile?>> GetByUserIdObservableTaskAsync(Guid userId, TransactionContext? transactionContext, GetSetMode getMode = GetSetMode.Mixed, IfUseLocalData<UserProfile>? ifUseLocalData = null)
        {
            return GetFirstOrDefaultObservableTaskAsync(
                localWhere: userProfile => userProfile.UserId == userId,
                remoteRequest: new UserProfileResGetByUserIdRequest(userId),
                transactionContext: transactionContext,
                getMode: getMode,
                ifUseLocalData: ifUseLocalData);
        }

        protected override async Task<IEnumerable<UserProfile>> GetFromRemoteAsync(IApiClient apiClient, ApiRequest request)
        {
            List<UserProfile> userProfiles = new List<UserProfile>();

            UserProfileRes? res = await apiClient.GetAsync<UserProfileRes>(request).ConfigureAwait(false);

            if (res != null)
            {
                userProfiles.Add(ToModel(res));
            }

            return userProfiles;
        }

        protected override Task AddToRemoteAsync(IApiClient apiClient, IEnumerable<UserProfile> models)
        {
            throw new NotImplementedException();
        }

        protected override async Task UpdateToRemoteAsync(IApiClient apiClient, IEnumerable<PropertyChangePack> cps)
        {
            foreach (PropertyChangePack cp in cps)
            {
                PatchRequest<UserProfileRes> patchRequest = new PatchRequest<UserProfileRes>(cp);

                await apiClient.SendAsync(patchRequest).ConfigureAwait(false);
            }
        }

        protected override Task DeleteFromRemoteAsync(IApiClient apiClient, IEnumerable<UserProfile> models)
        {
            throw new NotImplementedException();
        }

        private static UserProfile ToModel(UserProfileRes res)
        {
            return new UserProfile
            {
                Id = res.Id,
                UserId = res.UserId,
                NickName = res.NickName,
                Gender = res.Gender,
                BirthDay = res.BirthDay,
                AvatarFileName = res.AvatarFileName
            };
        }
    }
}