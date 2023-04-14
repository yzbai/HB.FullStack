using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Shared;

using Microsoft;

namespace HB.FullStack.Client
{
    public delegate bool IfUseLocalData<TModel>(ApiRequest request, IEnumerable<TModel> models) where TModel : ClientDbModel;

    public abstract class BaseRepo
    {
        protected IPreferenceProvider PreferenceProvider { get; }

        protected IApiClient ApiClient { get; }

        //protected IStatusManager StatusManager { get; }

        protected string LastUser => PreferenceProvider.UserId?.ToString() ?? "NotLogined";

        protected void EnsureLogined()
        {
            if (!PreferenceProvider.IsLogined())
            {
                throw ClientExceptions.NotLogined();
            }
        }

        protected static void EnsureApiNotReturnNull([ValidatedNotNull][NotNull] object? obj, string modelName)
        {
            if (obj == null)
            {
                throw CommonExceptions.ServerNullReturn(parameter: modelName);
            }
        }

        protected BaseRepo(IApiClient apiClient, IPreferenceProvider userPreferenceProvider/*, IStatusManager statusManager*/)
        {
            ApiClient = apiClient;
            PreferenceProvider = userPreferenceProvider;
            //StatusManager = statusManager;
        }
    }
}