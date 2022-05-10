﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public UpdateRequest() { }

        public UpdateRequest(IEnumerable<T> ress) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Put, true, null))
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res) : this(new T[] { res }) { }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(base.GetHashCode());

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}