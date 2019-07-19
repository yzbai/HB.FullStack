using HB.Framework.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace HB.Framework.Http.SDK
{
    public class ResourceRequest : ValidatableObject
    {
        [Required]
        public string DeviceId {
            get {
                return GetParameter(MobileInfoNames.DeviceId);
            }

            set {
                SetParameter(MobileInfoNames.DeviceId, value);
            }
        }

        //All use fields instead of Properties, for avoid mvc binding
        private readonly string productType;
        private readonly string apiVersion;
        private readonly HttpMethod httpMethod;
        private readonly bool needAuthenticate;
        private readonly string resourceName;
        private readonly string condition;
        private readonly IDictionary<string, string> headers = new Dictionary<string, string>();
        private readonly IDictionary<string, string> parameters = new Dictionary<string, string>();

        public string GetProductType()
        {
            return productType;
        }

        public string GetApiVersion()
        {
            return apiVersion;
        }

        public HttpMethod GetHttpMethod()
        {
            return httpMethod;
        }

        public bool GetNeedAuthenticate()
        {
            return needAuthenticate;
        }

        public string GetResourceName()
        {
            return resourceName;
        }

        public string GetCondition()
        {
            return condition;
        }

        protected string GetParameter(string name)
        {
            if (parameters.TryGetValue(name, out string value))
            {
                return value;
            }

            return null;
        }

        protected void SetParameter(string name, string value)
        {
            parameters[name] = value;
        }

        public void AddParameter(string name, string value)
        {
            if (parameters.ContainsKey(name))
            {
                throw new ArgumentException($"Request Already has a parameter named {name}");
            }

            parameters.Add(name, value);
        }

        public void AddHeader(string name, string value)
        {
            if (headers.ContainsKey(name))
            {
                throw new ArgumentException($"Request Already has a header named {name}");
            }

            headers.Add(name, value);
        }

        public IDictionary<string, string> GetParameters()
        {
            return parameters;
        }

        public IDictionary<string, string> GetHeaders()
        {
            return headers;
        }


        public ResourceRequest(string productType, string apiVersion, HttpMethod httpMethod, bool needAuthenticate, string resourceName, string condition = null)
        {
            this.productType = productType.ThrowIfNullOrEmpty(nameof(productType));
            this.apiVersion = apiVersion;
            this.httpMethod = httpMethod.ThrowIfNull(nameof(httpMethod));
            this.needAuthenticate = needAuthenticate;
            this.resourceName = resourceName;
            this.condition = condition;
        }
    }
}
