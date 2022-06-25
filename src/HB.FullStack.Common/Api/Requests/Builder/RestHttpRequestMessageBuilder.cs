using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组建方式Restful Api方式
    /// </summary>
    public class RestHttpRequestMessageBuilder : HttpRequestMessageBuilder
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

        public sealed override string GetUrl()
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
                if (Parent1ResName.IsNotNullOrEmpty())
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (Parent1ResId.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException(nameof(Parent1ResId));
                    }

                    stringBuilder.Append(Parent1ResName);
                    stringBuilder.Append('/');
                    stringBuilder.Append(Parent1ResId);

                    if (Parent2ResName.IsNotNullOrEmpty())
                    {
                        if (Parent2ResId.IsNullOrEmpty())
                        {
                            throw new ArgumentNullException(nameof(Parent2ResId));
                        }

                        stringBuilder.Append('/');
                        stringBuilder.Append(Parent2ResName);
                        stringBuilder.Append('/');
                        stringBuilder.Append(Parent2ResId);
                    }

                    return stringBuilder.ToString();
                }

                return null;
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

        public RestHttpRequestMessageBuilder(
            HttpMethodName httpMethod,
            HttpMethodOverrideMode httpMethodOverrideMode,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, httpMethodOverrideMode, apiAuthType)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }

        public RestHttpRequestMessageBuilder(
            HttpMethodName httpMethod,
            HttpMethodOverrideMode httpsMethodOverrideMode,
            string apiKeyName,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, httpsMethodOverrideMode, ApiAuthType.ApiKey, apiKeyName)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }
    }

    /// <summary>
    /// 从Res和ApiRequest中收集构建HttpRequestMessage的信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RestfulHttpRequestBuilder<T> : RestHttpRequestMessageBuilder where T : ApiResource2
    {
        public RestfulHttpRequestBuilder(ApiRequest apiRequest, HttpMethodOverrideMode httpMethodOverrideMode)
            : base(apiRequest.HttpMethodName, httpMethodOverrideMode, ApiAuthType.None, null, null, null, apiRequest.Condition)
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

            Parent1ResId = def.Parent1ResIdGetMethod?.Invoke(apiRequest, null)?.ToString();
            Parent2ResId = def.Parent2ResIdGetMethod?.Invoke(apiRequest, null)?.ToString();
        }
    }
}