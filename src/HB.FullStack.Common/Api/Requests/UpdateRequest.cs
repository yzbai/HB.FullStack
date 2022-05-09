using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// PUT /Ver/ResoruceCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public UpdateRequest()
        { }

        public UpdateRequest(IEnumerable<T> ress) : base(HttpMethodName.Put, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> ress) : base(apiKeyName, HttpMethodName.Put, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res) : this(new T[] { res }) { }

        public UpdateRequest(string apiKeyName, T res) : this(apiKeyName, new T[] { res }) { }

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

    public class UpdateRequest<T, TParent> : UpdateRequest<T> where T : ApiResource2 where TParent : ApiResource2
    {
        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public UpdateRequest() { }

        public UpdateRequest(Guid parentId, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            BuildInfo!.AddParent(paretnDef.ResName, parentId.ToString());
        }

        public UpdateRequest(Guid parentId, T res) : this(parentId, new T[] { res }) { }

        public UpdateRequest(string apiKeyName, Guid parentId, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            BuildInfo!.AddParent(paretnDef.ResName, parentId.ToString());
        }

        public UpdateRequest(string apiKeyName, Guid parentId, T res) : this(apiKeyName, parentId, new T[] { res }) { }
    }

    public class UpdateRequest<T, TParent1, TParent2> : UpdateRequest<T> where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    {
        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public UpdateRequest() { }

        public UpdateRequest(Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            BuildInfo!.AddParent(paretn1Def.ResName, parent1Id.ToString());

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            BuildInfo!.AddParent(paretn2Def.ResName, parent2Id.ToString());
        }

        public UpdateRequest(Guid parent1Id, Guid parent2Id, T res) : this(parent1Id, parent2Id, new T[] { res }) { }

        public UpdateRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, IEnumerable<T> ress) : base(apiKeyName, ress)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            BuildInfo!.AddParent(paretn1Def.ResName, parent1Id.ToString());

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            BuildInfo!.AddParent(paretn2Def.ResName, parent2Id.ToString());
        }

        public UpdateRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, T res) : this(apiKeyName, parent1Id, parent2Id, new T[] { res }) { }
    }
}