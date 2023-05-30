using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass] 
    public class RetrieveTests : DatabaseTestClass
    {
        [TestMethod]
        public async Task Test_Reader_Dispose<T>() where T : class, IDbModel
        {
            var models = await AddAndRetrieve<T>();

            Db.RetrieveAllAsync<T>(null);
        }
    }
}
