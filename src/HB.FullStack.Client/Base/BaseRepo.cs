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
using HB.FullStack.Client.Components.Users;

namespace HB.FullStack.Client.Base
{
    public delegate bool IfUseLocalData<TModel>(ApiRequest request, IEnumerable<TModel> models) where TModel : ClientDbModel;

    public abstract class BaseRepo
    {
        protected ITokenPreferences ClientPreferences { get; }

        protected IApiClient ApiClient { get; }

        protected void EnsureLogined()
        {
            if (!ClientPreferences.IsLogined())
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

        protected BaseRepo(IApiClient apiClient, ITokenPreferences clientPreferences)
        {
            ApiClient = apiClient;
            ClientPreferences = clientPreferences;
        }
    }
}