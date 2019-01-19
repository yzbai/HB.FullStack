using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace HB.Framework.Http.Security
{
    public static class InvalidTokenErrorMessages
    {
        private const string ErrorMessage_AUDIENCE_INVALID = "ErrorMessage_AUDIENCE_INVALID";
        private const string ErrorMessage_EXPIRED = "ACCESSTOKEN_EXPIRED";
        private const string ErrorMessage_ISSUER_INVALID = "ErrorMessage_ISSUER_INVALID";
        private const string ErrorMessage_LIFETIME_INVALID = "ErrorMessage_LIFETIME_INVALID";
        private const string ErrorMessage_NO_EXPIRATION = "ErrorMessage_NO_EXPIRATION";
        private const string ErrorMessage_NOT_VALID_YET = "ErrorMessage_NOT_VALID_YET";
        private const string ErrorMessage_SIGNATURE_INVALID = "ErrorMessage_SIGNATURE_INVALID";
        private const string ErrorMessage_SIGNATURE_KEY_NOT_FOUND = "ErrorMessage_SIGNATURE_KEY_NOT_FOUND";

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
                message = ErrorMessage_AUDIENCE_INVALID;
            }
            else if (ex is SecurityTokenInvalidIssuerException)
            {
                message = ErrorMessage_ISSUER_INVALID;
            }
            else if (ex is SecurityTokenNoExpirationException)
            {
                message = ErrorMessage_NO_EXPIRATION;
            }
            else if (ex is SecurityTokenInvalidLifetimeException)
            {
                message = ErrorMessage_LIFETIME_INVALID;
            }
            else if (ex is SecurityTokenNotYetValidException)
            {
                message = ErrorMessage_NOT_VALID_YET;
            }
            else if (ex is SecurityTokenExpiredException)
            {
                message = ErrorMessage_EXPIRED;
            }
            else if (ex is SecurityTokenSignatureKeyNotFoundException)
            {
                message = ErrorMessage_SIGNATURE_KEY_NOT_FOUND;
            }
            else if (ex is SecurityTokenInvalidSignatureException)
            {
                message = ErrorMessage_SIGNATURE_INVALID;
            }

            return message;
        }
    }
}