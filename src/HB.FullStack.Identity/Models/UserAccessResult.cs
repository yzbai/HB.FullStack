using HB.FullStack.Common;
using HB.FullStack.Identity.Models;

namespace HB.FullStack.Identity
{
    public class UserAccessResult : Model
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