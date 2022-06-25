using System;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组件方式是简单的传入。
    /// </summary>
    public class PlainHttpRequestMessageBuilder : HttpRequestMessageBuilder
    {
        public string PlainUrl { get; }

        public PlainHttpRequestMessageBuilder(
            HttpMethodName httpMethod,
            HttpMethodOverrideMode httpMethodOverrideMode,
            ApiAuthType apiAuthType,
            string plainUrl) : base(httpMethod, httpMethodOverrideMode, apiAuthType)
        {
            PlainUrl = plainUrl;
        }

        public PlainHttpRequestMessageBuilder(
            HttpMethodName httpMethod,
            HttpMethodOverrideMode httpMethodOverrideMode,
            string apiKeyName,
            string plainUrl) : base(httpMethod, httpMethodOverrideMode, ApiAuthType.ApiKey, apiKeyName)
        {
            PlainUrl = plainUrl;
        }

        public override string GetUrl()
        {
            return PlainUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), PlainUrl);
        }
    }
}