﻿using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class HttpRequestBuilder
    {
        internal ResBinding ResBinding { get; }
        internal ApiRequest Request { get; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public ApiRequestAuth Auth { get; }

        public EndpointSetting EndpointSetting => ResBinding.EndpointSetting!;

        public HttpRequestBuilder(ResBinding resBinding, ApiRequest request)
        {
            ResBinding = resBinding;
            Request = request;

            Auth = request.Auth.HasValue ? request.Auth.Value : request.ApiMethodName switch
            {
                ApiMethodName.Get => resBinding.ReadAuth,
                ApiMethodName.Post => resBinding.WriteAuth,
                ApiMethodName.Put => resBinding.WriteAuth,
                ApiMethodName.Delete => resBinding.WriteAuth,
                ApiMethodName.Patch => resBinding.WriteAuth,
                ApiMethodName.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        public void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = $"{ResBinding.EndpointSetting!.Challenge} {jwt}";
        }

        public void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public void SetDeviceId(string deviceId)
        {
            Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public void SetDeviceVersion(string deviceVersion)
        {
            Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

        public string GetUrl()
        {
            if (ResBinding.Type == ResBindingType.PlainUrl)
            {
                return ResBinding.BindingValue;
            }

            if (ResBinding.Type == ResBindingType.ControllerModel)
            {
                StringBuilder builder = new StringBuilder();

                //Version
                if (ResBinding.EndpointSetting!.Version.IsNotNullOrEmpty())
                {
                    builder.Append(ResBinding.EndpointSetting.Version);
                    builder.Append('/');
                }

                //ControllerModelName
                builder.Append(ResBinding.BindingValue);

                //Condition
                if (Request.Condition.IsNotNullOrEmpty())
                {
                    builder.Append('/');
                    builder.Append(Request.Condition);
                }

                return builder.ToString();
            }

            throw new NotImplementedException("Other ResBindingType not implemented.");
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(ResBinding.GetHashCode());
            hashCode.Add(Request.GetHashCode());

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }

            return hashCode.ToHashCode();
        }
    }
}