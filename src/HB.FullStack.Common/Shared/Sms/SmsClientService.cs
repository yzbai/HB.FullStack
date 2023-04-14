using System;
using System.Threading.Tasks;

using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Validate;

using Microsoft.Extensions.Options;


namespace HB.FullStack.Common.Shared.Sms
{
    public class SmsClientService : ISmsClientService
    {
        private readonly IApiClient _apiClient;

        public SmsClientService(IApiClient apiClient)
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