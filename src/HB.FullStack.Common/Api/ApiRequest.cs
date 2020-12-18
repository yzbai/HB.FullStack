#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiRequest : ValidatableObject
    {
        #region Common Parameters

        [Required]
        public string DeviceId { get; set; } = null!;

        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        [Required]
        public string DeviceVersion { get; set; } = null!;

        public string? PublicResourceToken { get; set; }

        #endregion Common Parameters

        #region Settings

        //All use fields & Get Methods instead of Properties, for avoid mvc binding & json searilize
        private readonly string _productName;

        private readonly string _apiVersion;
        private readonly HttpMethod _httpMethod;
        private readonly string _resourceName;
        private readonly string? _condition;
        private bool _needHttpMethodOveride = true;
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();

        /// <summary>
        /// 因为不会直接使用ApiRequest作为Api的请求参数，所以不用提供无参构造函数，而具体的子类需要提供
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="apiVersion"></param>
        /// <param name="httpMethod"></param>
        /// <param name="resourceName"></param>
        /// <param name="condition">同一Verb下的条件分支，比如在ApiController上标注的[HttpGet("BySms")],BySms就是condition</param>
        public ApiRequest(string productName, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition = null)
        {
            _productName = productName;
            _apiVersion = apiVersion;
            _httpMethod = httpMethod;
            _resourceName = resourceName;
            _condition = condition;
        }

        public string GetProductName()
        {
            return _productName;
        }

        public string GetApiVersion()
        {
            return _apiVersion;
        }

        public HttpMethod GetHttpMethod()
        {
            return _httpMethod;
        }

        public string GetResourceName()
        {
            return _resourceName;
        }

        public string? GetCondition()
        {
            return _condition;
        }

        public bool GetNeedHttpMethodOveride()
        {
            return _needHttpMethodOveride;
        }

        public void SetNeedHttpMethodOveride(bool isNeeded)
        {
            _needHttpMethodOveride = isNeeded;
        }

        public static string GetRandomStr()
        {
            return SecurityUtil.CreateRandomString(6);
        }

        public string? GetHeader(string name)
        {
            if (_headers.TryGetValue(name, out string value))
            {
                return value;
            }

            return null;
        }

        
        public void SetHeader(string name, string value)
        {
            _headers[name] = value;
        }

        public IDictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        #endregion Settings
    }
}