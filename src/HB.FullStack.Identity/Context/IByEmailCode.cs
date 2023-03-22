namespace HB.FullStack.Identity
{
    public interface IByEmailCode
    {
        string Email { get; }   
        string EmailCode { get; }
    }
}