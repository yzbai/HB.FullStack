using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System
{
    public static class ValidationResultsExtensions
    {
        public static bool ExistErrorOf(this IEnumerable<ValidationResult> valiationResults, string propertyName)
        {
            ThrowIf.Null(valiationResults, nameof(valiationResults));
            ThrowIf.NullOrEmpty(propertyName, nameof(propertyName));

            foreach (ValidationResult result in valiationResults)
            {
                if (result.MemberNames.Contains(propertyName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ExistError(this IEnumerable<ValidationResult> valiationResults)
        {
            ThrowIf.Null(valiationResults, nameof(valiationResults));

            return valiationResults.Any();
        }

        public static string? ErrorMessageOf(this IEnumerable<ValidationResult> validationResults, string propertyName)
        {
            ThrowIf.Null(validationResults, nameof(validationResults));
            ThrowIf.NullOrEmpty(propertyName, nameof(propertyName));

            foreach (ValidationResult result in validationResults)
            {
                if (result.MemberNames.Contains(propertyName))
                {
                    return result.ErrorMessage;
                }
            }

            return null;
        }
    }
}