using System.Threading.Tasks;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.Components.Sms
{
    public interface ISmsService
    {
        Task<SmsValidationCodeRes?> RequestVCodeAsync(string mobile);
    }
}