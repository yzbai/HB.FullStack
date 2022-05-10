﻿global using static HB.FullStack.CommonTests.Data.ApiConstants;

using HB.FullStack.Common;

using System;


namespace HB.FullStack.CommonTests.Data
{

    [ApiResource(ApiEndpointName, ApiVersion, nameof(BookRes), null)]
    public class BookRes : ApiResource2
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