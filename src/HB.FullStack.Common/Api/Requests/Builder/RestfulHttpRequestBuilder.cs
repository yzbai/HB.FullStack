using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.FullStack.Common.Api.Requests;

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

        public string? ModelName { get; set; }

        #endregion

        //#region 由Request决定

        //public Guid? ModelId { get; set; }

        //public string? Parent1ModelName { get; set; }

        //public string? Parent1ModelId { get; set; }

        //public string? Parent2ModelName { get; set; }

        //public string? Parent2ModelId { get; set; }

        //#endregion

        public override string GetUrl()
        {
            return Condition.IsNullOrEmpty()? $"{ApiVersion}/{ModelName}" : $"{ApiVersion}/{ModelName}/{Condition}";

            //string? parentSegment = GetParentSegment();

            //if (parentSegment == null && ModelId == null)
            //{
            //    return $"{ApiVersion}/{ModelName}/{Condition}";
            //}
            //else if (parentSegment == null && ModelId != null)
            //{
            //    return $"{ApiVersion}/{ModelName}/{ModelId}/{Condition}";
            //}
            //else if (parentSegment != null && ModelId == null)
            //{
            //    return $"{ApiVersion}/{parentSegment}/{ModelName}/{Condition}";
            //}
            //else //if(parentSegment != null && ModelId != null)
            //{
            //    return $"{ApiVersion}/{parentSegment}/{ModelName}/{ModelId}/{Condition}";
            //}
        }

        //string? GetParentSegment()
        //{
        //    if (Parent1ModelName.IsNotNullOrEmpty())
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        if (Parent1ModelId.IsNullOrEmpty())
        //        {
        //            throw new ArgumentNullException(nameof(Parent1ModelId));
        //        }

        //        stringBuilder.Append(Parent1ModelName);
        //        stringBuilder.Append('/');
        //        stringBuilder.Append(Parent1ModelId);

        //        if (Parent2ModelName.IsNotNullOrEmpty())
        //        {
        //            if (Parent2ModelId.IsNullOrEmpty())
        //            {
        //                throw new ArgumentNullException(nameof(Parent2ModelId));
        //            }

        //            stringBuilder.Append('/');
        //            stringBuilder.Append(Parent2ModelName);
        //            stringBuilder.Append('/');
        //            stringBuilder.Append(Parent2ModelId);
        //        }

        //        return stringBuilder.ToString();
        //    }

        //    return null;
        //}

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(base.GetHashCode());
            hashCode.Add(EndpointName);
            hashCode.Add(ApiVersion);
            hashCode.Add(Condition);
            hashCode.Add(ModelName);
            //hashCode.Add(ModelId);
            //hashCode.Add(Parent1ModelName);
            //hashCode.Add(Parent2ModelName);
            //hashCode.Add(Parent1ModelId);
            //hashCode.Add(Parent2ModelId);

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
            string? controllerModelName) : base(apiMethodName, auth, condition)
        {
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ModelName = controllerModelName;
        }
    }
}