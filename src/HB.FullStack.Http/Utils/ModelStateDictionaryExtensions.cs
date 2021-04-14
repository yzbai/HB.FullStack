using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System
{
    public static class ModelStateDictionaryExtensions
    {
        public static string? GetErrors(this ModelStateDictionary dicts)
        {

            if (dicts.IsNullOrEmpty())
            {
                return null;
            }

            Dictionary<string, IEnumerable<string>> errorDict = new Dictionary<string, IEnumerable<string>>();

            foreach (KeyValuePair<string, ModelStateEntry> states in dicts)
            {
                if (states.Value.ValidationState == ModelValidationState.Invalid)
                {
                    errorDict.Add(states.Key, states.Value.Errors.Select(me => me.ErrorMessage));
                }
            }

            return SerializeUtil.TryToJson(errorDict);
        }
    }
}
