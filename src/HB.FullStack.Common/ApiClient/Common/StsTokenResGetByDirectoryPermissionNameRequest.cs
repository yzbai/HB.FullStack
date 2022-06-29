﻿using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class StsTokenResGetByDirectoryPermissionNameRequest : ApiRequest
    {
        private readonly string _requestUrl = null!;

        /// <summary>
        /// DirectoryPermissionName
        /// </summary>
        [Required]
        public string DirectoryPermissionName { get; set; } = null!;

        public string? RegexPlaceHolderValue { get; set; }

        /// <summary>
        /// OnlyForJsonConstructor
        /// </summary>
        public StsTokenResGetByDirectoryPermissionNameRequest() { }

        public StsTokenResGetByDirectoryPermissionNameRequest(ApiRequestAuth auth, string requestUrl, string directoryPermissionName, string? regexPlaceHolderValue) : base(ApiMethodName.Get, auth, null)
        {
            DirectoryPermissionName = directoryPermissionName;
            _requestUrl = requestUrl;

            RegexPlaceHolderValue = regexPlaceHolderValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), DirectoryPermissionName, RegexPlaceHolderValue);
        }

        protected override HttpRequestBuilder CreateHttpRequestBuilder()
        {
            return new PlainUrlHttpRequestBuilder(ApiMethodName, Auth, _requestUrl);
        }
    }
}