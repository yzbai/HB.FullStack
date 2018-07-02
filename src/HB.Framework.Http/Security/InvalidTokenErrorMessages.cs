using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.Framework.Http.Security
{
    public class InvalidTokenErrorMessages
    {
        public static readonly string AUDIENCE_INVALID          = "AUDIENCE_INVALID";
        public static readonly string ISSUER_INVALID            = "ISSUER_INVALID";
        public static readonly string NO_EXPIRATION             = "NO_EXPIRATION";
        public static readonly string LIFETIME_INVALID          = "LIFETIME_INVALID";
        public static readonly string NOT_VALID_YET             = "NOT_VALID_YET";
        public static readonly string EXPIRED                   = "EXPIRED";
        public static readonly string SIGNATURE_KEY_NOT_FOUND   = "SIGNATURE_KEY_NOT_FOUND";
        public static readonly string SIGNATURE_INVALID         = "SIGNATURE_INVALID";

        public static string GetErrorMessage(Exception authFailure)
        {
            IEnumerable<Exception> exceptions;

            if (authFailure is AggregateException)
            {
                var agEx = authFailure as AggregateException;
                exceptions = agEx.InnerExceptions;
            }
            else
            {
                exceptions = new[] { authFailure };
            }

            if (exceptions.Count() == 0)
            {
                return string.Empty;
            }

            string message = string.Empty;

            Exception ex = exceptions.ElementAt(0);

            // Order sensitive, some of these exceptions derive from others
            // and we want to display the most specific message possible.
            if (ex is SecurityTokenInvalidAudienceException)
            {
                message = InvalidTokenErrorMessages.AUDIENCE_INVALID;
            }
            else if (ex is SecurityTokenInvalidIssuerException)
            {
                message = InvalidTokenErrorMessages.ISSUER_INVALID;
            }
            else if (ex is SecurityTokenNoExpirationException)
            {
                message = InvalidTokenErrorMessages.NO_EXPIRATION;
            }
            else if (ex is SecurityTokenInvalidLifetimeException)
            {
                message = InvalidTokenErrorMessages.LIFETIME_INVALID;
            }
            else if (ex is SecurityTokenNotYetValidException)
            {
                message = InvalidTokenErrorMessages.NOT_VALID_YET;
            }
            else if (ex is SecurityTokenExpiredException)
            {
                message = InvalidTokenErrorMessages.EXPIRED;
            }
            else if (ex is SecurityTokenSignatureKeyNotFoundException)
            {
                message = InvalidTokenErrorMessages.SIGNATURE_KEY_NOT_FOUND;
            }
            else if (ex is SecurityTokenInvalidSignatureException)
            {
                message = InvalidTokenErrorMessages.SIGNATURE_INVALID;
            }

            return message;
        }
    }
}