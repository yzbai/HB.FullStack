global using static HB.FullStack.CommonTests.Data.ApiConstants;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Api.Resources;

using System;


namespace HB.FullStack.CommonTests.Data
{

    [ApiResourceBinding(ApiEndpointName, ApiVersion, nameof(BookRes))]
    public class BookRes : ApiResource
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public double Price { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Title, Price);
        }
    }
}