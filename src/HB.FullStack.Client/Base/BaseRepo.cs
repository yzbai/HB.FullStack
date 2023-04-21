/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;

using Microsoft;
using HB.FullStack.Client.Abstractions;

namespace HB.FullStack.Client.Base
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