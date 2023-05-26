using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.BaseTest.DapperMapper;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class DatabaseTestClass : BaseTestClass
    {
        internal async Task<IList<T>> AddAndRetrieve<T>(int count = 50, Action<int, T>? additionalAction = null) where T : IDbModel
        {
            var models = Mocker.Mock<T>(count, additionalAction);

            IList<T> rts;

            TransactionContext addTrans = await Trans.BeginTransactionAsync<T>();

            try
            {
                await Db.AddAsync(models, "", addTrans);

                rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), addTrans);

                await addTrans.CommitAsync();
            }
            catch
            {
                await addTrans.RollbackAsync();
                throw;
            }

            Assert.IsTrue(SerializeUtil.ToJson(models) == SerializeUtil.ToJson(rts));

            return rts;
        }


    }
}