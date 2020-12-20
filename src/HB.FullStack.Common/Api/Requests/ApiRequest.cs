#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiRequest : ValidatableObject
    {
        #region Common Parameters

        public string DeviceId { get; set; } = null!;

        public DeviceInfos DeviceInfos { get; set; } = null!;

        public string DeviceVersion { get; set; } = null!;

        public string? PublicResourceToken { get; set; }

        #endregion Common Parameters


        //All use fields & Get Methods instead of Properties, for avoid mvc binding & json searilize
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected string _endpointName = null!;
        protected string _apiVersion = null!;
        protected string _resourceName = null!;

        protected HttpMethod _httpMethod = null!;
        protected string? _condition;
        protected bool _needHttpMethodOveride = true;
        protected readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
#pragma warning restore CA1051 // Do not declare visible instance fields

        protected ApiRequest() { }

        protected ApiRequest(string endPointName, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition)
        {
            _endpointName = endPointName;
            _apiVersion = apiVersion;
            _httpMethod = httpMethod;
            _resourceName = resourceName;
            _condition = condition;
        }

        public string GetProductName()
        {
            return _endpointName;
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
    }

    public abstract class ApiRequest<T> : ApiRequest where T : Resource
    {

        /// <summary>
        /// 因为不会直接使用ApiRequest作为Api的请求参数，所以不用提供无参构造函数，而具体的子类需要提供
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="condition">同一Verb下的条件分支，比如在ApiController上标注的[HttpGet("BySms")],BySms就是condition</param>
        public ApiRequest(HttpMethod httpMethod, string? condition)
        {
            ResourceDef def = ResourceDefFactory.Get<T>();

            _endpointName = def.EndpointName;
            _apiVersion = def.ApiVersion;
            _resourceName = def.Name;
            _httpMethod = httpMethod;
            _condition = condition;
        }

        public ApiRequest(string endPointName, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition) : base(endPointName, apiVersion, httpMethod, resourceName, condition)
        {
        }
    }
}