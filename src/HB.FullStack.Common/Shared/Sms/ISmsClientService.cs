using System.Threading.Tasks;



namespace HB.FullStack.Common.Shared.Sms
{
    public interface ISmsClientService
    {
        Task<SmsValidationCodeRes?> RequestVCodeAsync(string mobile);
    }
}