using System.Threading.Tasks;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.Services.Sms
{
    public interface ISmsService
    {
        Task<SmsValidationCodeRes?> RequestVCodeAsync(string mobile);
    }
}