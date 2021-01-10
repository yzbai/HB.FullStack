using HB.Component.Identity.Entities;

namespace HB.Component.Authorization
{
    public class SignInResult
    {
        //result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, userToken, context.SignToWhere).ConfigureAwait(false);
        //result.RefreshToken = userToken.RefreshToken;
        //        result.NewUserCreated = newUserCreated;
        //        result.CurrentUser = user;

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public bool NewUserCreated { get; set; }

        public User CurrentUser { get; set; }

        public SignInResult(string accessToken, string refreshToken, bool newUserCreated, User currentUser)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            NewUserCreated = newUserCreated;
            CurrentUser = currentUser;
        }
    }
}