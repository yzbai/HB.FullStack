namespace HB.FullStack.Server.Identity
{
    public interface IByEmailCode
    {
        string Email { get; }   
        string EmailCode { get; }
    }
}