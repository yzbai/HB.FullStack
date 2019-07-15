using HB.Framework.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HB.Framework.Http.SDK
{
    public class ResourceRequest : ValidatableObject
    {
        public string DeviceId {
            get {
                return GetParameter("DeviceId");
            }

            set {
                SetParameter("DeviceId", value);
            }
        }
        public string DeviceType {
            get {
                return GetParameter("DeviceId");
            }

            set {
                SetParameter("DeviceId", value);
            }
        }
        public string DeviceVersion {
            get {
                return GetParameter("DeviceId");
            }

            set {
                SetParameter("DeviceId", value);
            }
        }
        public string DeviceAddress {
            get {
                return GetParameter("DeviceId");
            }

            set {
                SetParameter("DeviceId", value);
            }
        }

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

        public IDictionary<string, string> GetHeaders()
        {
            return headers;
        }

        public IDictionary<string, string> GetParameters()
        {
            return parameters;
        }

        protected string GetParameter(string name)
        {
            if (GetParameters().TryGetValue(name, out string value))
            {
                return value;
            }

            return null;
        }

        protected void SetParameter(string name, string value)
        {
            GetParameters()[name] = value;
        }

        public void AddParameter(string name, string value)
        {
            if (GetParameters().ContainsKey(name))
            {
                throw new ArgumentException($"Request Already has a parameter named {name}");
            }

            GetParameters().Add(name, value);
        }

        public ResourceRequest(string productType, string apiVersion, HttpMethod httpMethod, bool needAuthenticate, string resourceName, string condition = null)
        {
            this.productType = productType.ThrowIfNullOrEmpty(nameof(productType));
            this.apiVersion = apiVersion.ThrowIfNullOrEmpty(nameof(apiVersion));
            this.httpMethod = httpMethod.ThrowIfNull(nameof(httpMethod));
            this.needAuthenticate = needAuthenticate;
            this.resourceName = resourceName.ThrowIfNullOrEmpty(nameof(resourceName));
            this.condition = condition.ThrowIfNullOrEmpty(nameof(condition));
        }
    }
}
