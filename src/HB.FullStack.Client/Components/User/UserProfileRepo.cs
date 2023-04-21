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
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.Components.User
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
            IPreferenceProvider preferenceProvider) : base(logger, clientModelSettingFactory, database, apiClient, syncManager, clientEvents, preferenceProvider)
        { }

        //protected override UserProfile ToModel(UserProfileRes res) => ResMapper.ToUserProfile(res);

        //protected override UserProfileRes ToResource(UserProfile model) => ResMapper.ToUserProfileRes(model);

        internal Task<UserProfile?> GetByUserIdAsync(Guid userId, TransactionContext? transactionContext, GetSetMode getMode = GetSetMode.Mixed)
        {
            return GetFirstOrDefaultAsync(
                localWhere: userProfile => userProfile.UserId == userId,
                remoteRequest: new UserProfileResGetByUserIdRequest(userId),
                transactionContext: transactionContext,
                getMode: getMode);
        }

        protected override Task<IEnumerable<UserProfile>> GetFromRemoteAsync(IApiClient apiClient, ApiRequest request)
        {
            throw new NotImplementedException();
        }

        protected override Task AddToRemoteAsync(IApiClient apiClient, IEnumerable<UserProfile> models)
        {
            throw new NotImplementedException();
        }

        protected override Task UpdateToRemoteAsync(IApiClient apiClient, IEnumerable<PropertyChangePack> cps)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteFromRemoteAsync(IApiClient apiClient, IEnumerable<UserProfile> models)
        {
            throw new NotImplementedException();
        }
    }
}