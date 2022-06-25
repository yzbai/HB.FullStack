using System;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组件方式是简单的传入。
    /// </summary>
    public class PlainUrlApiRequestBuilder : ApiRequestBuilder
    {
        public string PlainUrl { get; }

        public PlainUrlApiRequestBuilder(
            HttpMethodName httpMethod,
            ApiRequestAuth auth,
            string plainUrl) : base(httpMethod, auth)
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