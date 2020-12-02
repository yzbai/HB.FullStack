using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System
{
    public static class ModelStateDictionaryExtensions
    {
        public static IDictionary<string, IEnumerable<string>> GetErrors(this ModelStateDictionary dicts)
        {
            Dictionary<string, IEnumerable<string>> errorDict = new Dictionary<string, IEnumerable<string>>();

            if (dicts.IsNullOrEmpty())
            {
                return errorDict;
            }

            foreach (KeyValuePair<string, ModelStateEntry> states in dicts)
            {
                if (states.Value.ValidationState == ModelValidationState.Invalid)
                {
                    errorDict.Add(states.Key, states.Value.Errors.Select(me => me.ErrorMessage));
                }
            }

            return errorDict;
        }
    }
}
