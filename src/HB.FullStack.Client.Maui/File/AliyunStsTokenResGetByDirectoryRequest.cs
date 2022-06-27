using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Client.Maui.File
{
    public class AliyunStsTokenResGetByDirectoryRequest : ApiRequest
    {
        private readonly string _requestUrl = null!;

        /// <summary>
        /// 需求Directory的权限,结尾不带slash
        /// </summary>
        [Required]
        public string Directory { get; set; } = null!;

        /// <summary>
        /// OnlyForJsonConstructor
        /// </summary>
        public AliyunStsTokenResGetByDirectoryRequest() { }

        public AliyunStsTokenResGetByDirectoryRequest(string directory, string requestUrl) : base(ApiMethodName.Get, ApiRequestAuth.JWT, null)
        {
            Directory = directory;
            _requestUrl = requestUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Directory);
        }

        protected override HttpRequestBuilder CreateHttpRequestBuilder()
        {
            return new PlainUrlHttpRequestBuilder(ApiMethodName, Auth, _requestUrl);
        }
    }
}