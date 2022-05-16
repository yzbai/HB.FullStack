using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 强调Url的组建方式Restful Api方式
    /// </summary>
    public class RestfulHttpRequestBuilder : HttpRequestBuilder
    {
        public string? EndpointName { get; set; }

        public string? ApiVersion { get; set; }

        public string? Condition { get; set; }

        public string? ResName { get; set; }

        public Guid? ResId { get; set; }

        private IList<ResParent> Parents { get; } = new List<ResParent>();

        public string? Parent1ResName
        {
            get
            {
                if (Parents.Any())
                {
                    return Parents[0].ResName;
                }
                return null;
            }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    return;
                }

                if (Parents.Any())
                {
                    Parents[0].ResName = value;
                }
                else
                {
                    AddParent(value, null);
                }
            }
        }

        public string? Parent1ResId
        {
            get
            {
                if (Parents.Any())
                {
                    return Parents[0].ResId;
                }

                return null;
            }
            set
            {
                if (Parents.Any())
                {
                    Parents[0].ResId = value;
                }
            }
        }

        public void AddParent(string parentResName, string? parentResId)
        {
            Parents.Add(new ResParent { ResName = parentResName, ResId = parentResId });
        }

        protected sealed override string GetUrlCore()
        {
            string? parentSegment = GetParentSegment(Parents);

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

            static string? GetParentSegment(IList<ResParent> lst)
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
                    stringBuilder.Append(parent.ResId);
                    stringBuilder.Append('/');
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

            foreach (ResParent parent in Parents)
            {
                hashCode.Add(parent.ResId);
                hashCode.Add(parent.ResName);
            }

            return hashCode.ToHashCode();
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, needHttpMethodOveride, apiAuthType)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }

        public RestfulHttpRequestBuilder(
            HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            string apiKeyName,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition) : base(httpMethod, needHttpMethodOveride, ApiAuthType.ApiKey, apiKeyName)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }

        class ResParent
        {
            public string ResName { get; set; } = null!;
            public string? ResId { get; set; }
        }
    }

    public class RestfulHttpRequestBuilder<T> : RestfulHttpRequestBuilder where T : ApiResource2
    {
        public RestfulHttpRequestBuilder(HttpMethodName httpMethod, bool needHttpMethodOveride, string? condition)
            : base(httpMethod, needHttpMethodOveride, ApiAuthType.None, null, null, null, condition)
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
        }
    }
}