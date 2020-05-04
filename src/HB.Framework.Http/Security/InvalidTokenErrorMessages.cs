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
        private const string _errorMessage_AUDIENCE_INVALID = "ErrorMessage_AUDIENCE_INVALID";
        private const string _errorMessage_EXPIRED = "ACCESSTOKEN_EXPIRED";
        private const string _errorMessage_ISSUER_INVALID = "ErrorMessage_ISSUER_INVALID";
        private const string _errorMessage_LIFETIME_INVALID = "ErrorMessage_LIFETIME_INVALID";
        private const string _errorMessage_NO_EXPIRATION = "ErrorMessage_NO_EXPIRATION";
        private const string _errorMessage_NOT_VALID_YET = "ErrorMessage_NOT_VALID_YET";
        private const string _errorMessage_SIGNATURE_INVALID = "ErrorMessage_SIGNATURE_INVALID";
        private const string _errorMessage_SIGNATURE_KEY_NOT_FOUND = "ErrorMessage_SIGNATURE_KEY_NOT_FOUND";

        public static string GetErrorMessage(Exception authFailure)
        {
            if (authFailure == null)
            {
                return "";
            }

            IEnumerable<Exception> exceptions;

            if (authFailure is AggregateException)
            {
                AggregateException agEx = (AggregateException)authFailure;
                exceptions = agEx.InnerExceptions;
            }
            else
            {
                exceptions = new[] { authFailure };
            }

            if (!exceptions.Any())
            {
                return string.Empty;
            }

            string message = string.Empty;

            Exception ex = exceptions.ElementAt(0);

            // Order sensitive, some of these exceptions derive from others
            // and we want to display the most specific message possible.
            if (ex is SecurityTokenInvalidAudienceException)
            {
                message = _errorMessage_AUDIENCE_INVALID;
            }
            else if (ex is SecurityTokenInvalidIssuerException)
            {
                message = _errorMessage_ISSUER_INVALID;
            }
            else if (ex is SecurityTokenNoExpirationException)
            {
                message = _errorMessage_NO_EXPIRATION;
            }
            else if (ex is SecurityTokenInvalidLifetimeException)
            {
                message = _errorMessage_LIFETIME_INVALID;
            }
            else if (ex is SecurityTokenNotYetValidException)
            {
                message = _errorMessage_NOT_VALID_YET;
            }
            else if (ex is SecurityTokenExpiredException)
            {
                message = _errorMessage_EXPIRED;
            }
            else if (ex is SecurityTokenSignatureKeyNotFoundException)
            {
                message = _errorMessage_SIGNATURE_KEY_NOT_FOUND;
            }
            else if (ex is SecurityTokenInvalidSignatureException)
            {
                message = _errorMessage_SIGNATURE_INVALID;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = authFailure.Message;
            }

            return message;
        }
    }
}