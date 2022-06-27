
using HB.FullStack.Common.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace HB.FullStack.Common.Test
{
    public class TestRequestHandler
    {
        readonly Regex _comparisonRegex;

        readonly List<string> _urlParameterNames = new();

        public TestRequestHandler(
            string url,
            ApiMethodName httpMethod,
            Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>?> handlerAction
        )
        {
            Url = url;
            HttpMethod = httpMethod.ToHttpMethodString();
            HandlerAction = handlerAction;

            _comparisonRegex = CreateComparisonRegex(url);
        }

        public TestRequestHandler(string url, Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>?> handlerAction)
            : this(url, ApiMethodName.None, handlerAction) { }

        string Url { get; }
        string HttpMethod { get; }
        internal Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>?> HandlerAction { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "<Pending>")]
        Regex CreateComparisonRegex(string url)
        {
            var regexString = Regex.Escape(url).Replace(@"\{", "{");

            regexString += regexString.EndsWith("/", StringComparison.InvariantCulture) ? "?" : "/?";
            regexString = (regexString.StartsWith("/", StringComparison.InvariantCulture) ? "^" : "^/") + regexString;

            var regex = new Regex(@"{(.*?)}");

            foreach (Match match in regex.Matches(regexString))
            {
                regexString = regexString.Replace(match.Value, @"(.*?)");
                _urlParameterNames.Add(match.Groups[1].Value);
            }

            regexString += !regexString.Contains(@"\?") ? @"(\?.*)?$" : "$";

            return new Regex(regexString);
        }

        public bool TryMatchUrl(string? rawUrl, string httpMethod, out Dictionary<string, string>? parameters)
        {
            if(rawUrl == null)
            {
                parameters = null;
                return false;
            }

            var match = _comparisonRegex.Match(rawUrl);

            var isMethodMatched = HttpMethod == null || HttpMethod.Split(',').Contains(httpMethod, StringComparer.InvariantCultureIgnoreCase);

            if (!match.Success || !isMethodMatched)
            {
                parameters = null;
                return false;
            }

            parameters = new Dictionary<string, string>();

            for (var i = 0; i < _urlParameterNames.Count; i++)
                parameters[_urlParameterNames[i]] = match.Groups[i + 1].Value;
            return true;
        }
    }
}
