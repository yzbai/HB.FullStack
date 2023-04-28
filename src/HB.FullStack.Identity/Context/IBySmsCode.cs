namespace HB.FullStack.Server.Identity
{
    public interface IBySmsCode
    {
        string Mobile { get; }

        string SmsCode { get; }
    }
}