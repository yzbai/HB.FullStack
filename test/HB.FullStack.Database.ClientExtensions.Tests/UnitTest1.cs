using System;
using HB.FullStack.Database.ClientExtension;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HB.FullStack.Database.ClientExtensions.Tests
{
    public class UnitTest1 : IClassFixture<ServiceFixture_Sqlite>
    {
        private readonly IDatabaseClientExtensions _databaseExtensions;

        public UnitTest1(ServiceFixture_Sqlite serviceFixture)
        {
            _databaseExtensions = serviceFixture.ServiceProvider.GetRequiredService<IDatabaseClientExtensions>();
        }
        [Fact]
        public void Test1()
        {

        }
    }
}
