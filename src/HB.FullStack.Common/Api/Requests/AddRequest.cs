﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : Resource
    {
        public AddRequest() : base(HttpMethod.Post, null) { }

        public AddRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Post, null) { }

        [CollectionNotEmpty]
        [IdBarrier]
        public List<T> Resources { get; set; } = new List<T>();

        public override int GetHashCode()
        {
            return ((ApiRequest)this).GetHashCode();
        }
    }

}