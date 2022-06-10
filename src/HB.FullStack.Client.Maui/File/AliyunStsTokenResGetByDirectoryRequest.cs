using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Client.Maui.File
{
    public class AliyunStsTokenResGetByDirectoryRequest : ApiRequest
    {
        /// <summary>
        /// 需求Directory的权限,结尾不带slash
        /// </summary>
        [Required]
        public string Directory { get; set; } = null!;

        /// <summary>
        /// OnlyForJsonConstructor
        /// </summary>
        public AliyunStsTokenResGetByDirectoryRequest() { }

        public AliyunStsTokenResGetByDirectoryRequest(string directory, string requestUrl) : base(new PlainUrlHttpRequestBuilder(HttpMethodName.Get, true, ApiAuthType.Jwt, requestUrl))
        {
            Directory = directory;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Directory);
        }
    }
}