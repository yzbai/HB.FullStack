using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组建方式Restful Api方式
    /// </summary>
    public class RestfulHttpRequestBuilder : HttpRequestMessageBuilder
    {
        public string? EndpointName { get; set; }

        public string? ApiVersion { get; set; }

        public string? Condition { get; set; }

        public string? ResName { get; set; }

        public Guid? ResId { get; set; }

        public string? Parent1ResName { get; set; }

        public string? Parent1ResId { get; set; }

        public string? Parent2ResName { get; set; }

        public string? Parent2ResId { get; set; }

        protected sealed override string GetUrlCore()
        {
            string? parentSegment = GetParentSegment();

            if (parentSegment == null && ResId == null)
            {
                return $"{ApiVersion}/{ResName}/{Condition}";
            }
            else if (parentSegment == null && ResId != null)
            {
                return $"{ApiVersion}/{ResName}/{ResId}/{Condition}";
            }
            else if (parentSegment != null && ResId == null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{Condition}";
            }
            else //if(parentSegment != null && ResId != null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{ResId}/{Condition}";
            }

            string? GetParentSegment()
            {
                if (lst.IsNullOrEmpty())
                {
                    return null;
                }

                StringBuilder stringBuilder = new StringBuilder();

                foreach (ResParent parent in lst)
                {
                    stringBuilder.Append(parent.ResName);
                    stringBuilder.Append('/');

                    if (parent.ResId.IsNotNullOrEmpty())
                    {
                        stringBuilder.Append(parent.ResId);
                        stringBuilder.Append('/');
                    }
                }

                return stringBuilder.RemoveLast().ToString();
            }
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(base.GetHashCode());
            hashCode.Add(EndpointName);
            hashCode.Add(ApiVersion);
            hashCode.Add(Condition);
            hashCode.Add(ResName);
            hashCode.Add(ResId);
            hashCode.Add(Parent1ResName);
            hashCode.Add(Parent2ResName);
            hashCode.Add(Parent1ResId);
            hashCode.Add(Parent2ResId);

            return hashCode.ToHashCode();
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, apiAuthType)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            string apiKeyName,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, ApiAuthType.ApiKey, apiKeyName)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }
    }

    public class RestfulHttpRequestBuilder<T> : RestfulHttpRequestBuilder where T : ApiResource2
    {
        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, string? condition)
            : base(httpMethod, ApiAuthType.None, null, null, null, condition)
        {
            ApiResourceDef? def = ApiResourceDefFactory.Get<T>();

            if (def == null)
            {
                throw ApiExceptions.LackApiResourceAttribute(typeof(T).FullName);
            }

            EndpointName = def.EndpointName;
            ApiVersion = def.Version;
            AuthType = def.AuthType;
            ResName = def.ResName;
            ApiKeyName = def.ApiKeyName;

            Parent1ResName = def.Parent1ResName;
            Parent2ResName = def.Parent2ResName;
        }
    }
}