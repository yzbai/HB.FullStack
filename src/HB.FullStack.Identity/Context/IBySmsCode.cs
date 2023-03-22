namespace HB.FullStack.Identity
{
    public interface IBySmsCode
    {
        string Mobile { get; }

        string SmsCode { get; }
    }
}