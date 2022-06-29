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
        #region 由Resource决定, 即ApiResourceAttribute, Parent1ResIdAttribute, Parent2ResIdAttribute

        public string? EndpointName { get; set; }

        public string? ApiVersion { get; set; }

        public string? ResName { get; set; }

        public Guid? ResId { get; set; }

        public string? Parent1ResName { get; set; }

        public string? Parent1ResId { get; set; }

        public string? Parent2ResName { get; set; }

        public string? Parent2ResId { get; set; }

        #endregion

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

        /// <summary>
        /// 需要的最小化信息
        /// </summary>
        public RestfulHttpRequestBuilder(
            ApiMethodName apiMethodName,
            ApiRequestAuth auth,
            string? condition,
            string? endPointName,
            string? apiVersion,
            string? resName) : base(apiMethodName, auth, condition)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
        }
    }
}