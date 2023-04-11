using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared.SignInReceipt;

namespace HB.FullStack.Common.ApiClient
{
    public static class IApiClientSignInReceiptExtensions
    {
        public static async Task RegisterByLoginNameAsync(this IApiClient apiClient, string loginName, string audience, string password)
        {
            SignInReceiptRegisterByLoginNameRequest registerRequest = new SignInReceiptRegisterByLoginNameRequest(
                loginName, 
                audience, 
                password, 
                apiClient.PreferenceProvider.DeviceInfos);

            await apiClient.SendAsync(registerRequest).ConfigureAwait(false);
        }
    }
}
