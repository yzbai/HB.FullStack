using System;
using System.Threading.Tasks;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared.Resources;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Options;


namespace HB.FullStack.Client.Services.Sms
{
    public class SmsService : ISmsService
    {
        private readonly IApiClient _apiClient;

        public SmsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<SmsValidationCodeRes?> RequestVCodeAsync(string mobile)
        {
            SmsValidationCodeResGetByMobileRequest request = new SmsValidationCodeResGetByMobileRequest(mobile);

            //TODO: use Capthca later
            //return await _apiClient.GetWithCaptchaCheckAsync<SmsValidationCodeRes>(request);
            return await _apiClient.GetAsync<SmsValidationCodeRes>(request);
        }
    }
}