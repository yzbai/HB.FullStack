using System;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组件方式是简单的传入。
    /// </summary>
    public class PlainUrlHttpRequestBuilder : HttpRequestBuilder
    {
        public string PlainUrl { get; }

        public PlainUrlHttpRequestBuilder(ApiMethodName apiMethodName, ApiRequestAuth auth, string plainUrl) 
            : base(apiMethodName, auth, null)
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