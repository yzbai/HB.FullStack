using HB.FullStack.Identity.Entities;
using HB.FullStack.Identity.ModelObjects;

namespace HB.FullStack.Identity
{
    public class UserAccessResult
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public User CurrentUser { get; set; }

        public UserAccessResult(string accessToken, string refreshToken, User currentUser)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            CurrentUser = currentUser;
        }
    }
}