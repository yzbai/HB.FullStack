using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Api;

namespace HB.FullStack.Client.Api
{
    public static class ApiClientExtensions
    {
        public static Task<T> SendAsync<T>(this ApiRequest request, IApiClient apiClient) where T : class
        {
            return apiClient.SendAsync<T>(request);
        }

        public static Task SendAsync(this ApiRequest request, IApiClient apiClient)
        {
            return apiClient.SendAsync(request);
        }
    }
}
