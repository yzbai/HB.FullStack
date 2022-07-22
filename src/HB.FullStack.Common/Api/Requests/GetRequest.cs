using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public class GetRequest<T> : ApiRequest where T : ApiResource
    {
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        /// <summary>
        /// 包含哪些子资源
        /// </summary>
        public string? Includes { get; set; }

        [OnlyForJsonConstructor]
        public GetRequest() { }

        public GetRequest(string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Get, auth, condition) { }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(Page);
            hashCode.Add(PerPage);
            hashCode.Add(OrderBys);

            return hashCode.ToHashCode();
        }

        public void OrderBy(params Expression<Func<T, object>>[]? orderBys)
        {
            if (orderBys.IsNullOrEmpty())
            {
                return;
            }

            StringBuilder orderByBuilder = new StringBuilder();

            foreach (Expression<Func<T, object>> orderBy in orderBys)
            {
                string orderByName = ((MemberExpression)orderBy.Body).Member.Name;
                orderByBuilder.Append(orderByName);
                orderByBuilder.Append(',');
            }

            orderByBuilder.RemoveLast();

            OrderBys = orderByBuilder.ToString();
        }

        public GetRequest<T> Include<TRes>() where TRes : ApiResource
        {
            string resName = typeof(TRes).Name;

            if (Includes == null)
            {
                Includes = resName;
            }
            else
            {
                Includes += $",{resName}";
            }

            return this;
        }
    }

    public class GetByIdsRequest<T> : GetRequest<T> where T : ApiResource
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        [OnlyForJsonConstructor]
        public GetByIdsRequest() { }

        public GetByIdsRequest(string resName, ApiRequestAuth auth, params Guid[] ids) : base(resName, auth, "ByIds")
        {
            ids.AddRange(ids);
        }
    }

    public class GetByIdRequest<T> : GetRequest<T> where T : ApiResource
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        [OnlyForJsonConstructor]
        public GetByIdRequest() { }

        public GetByIdRequest(string resName, ApiRequestAuth auth, Guid id) : base(resName, auth, "ById")
        {
            Id = id;
        }
    }
}