using System.Threading.Tasks;

using HB.FullStack.Identity.Entities;

namespace HB.FullStack.Identity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
    public interface IIdentityService
    {
        //Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser);
        //Task<User?> GetUserByLoginNameAsync(string loginName);
        //Task<User?> GetUserByMobileAsync(string mobile);
        //Task<User?> GetUserByUserGuidAsync(string userGuid);
    }
}