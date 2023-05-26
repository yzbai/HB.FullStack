using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.BaseTest.Models;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class JoinTests : DatabaseTestClass
    {
        private async Task Test_JoinTest_ThreeTable_Core<TA, TB, TAB>() where TA : IJoinTest_A where TB : IJoinTest_B where TAB : IJoinTest_AB
        {
            var rtA = await AddAndRetrieve<TA>(10);
            var rtB = await AddAndRetrieve<TB>(10);

            var rtAB = await AddAndRetrieve<TAB>(10, (i, ab) =>
            {
                ab.AId = rtA[i].Id;
                ab.BId = rtB[i].Id;
            });


            var from = Db
                .From<TA>()
                .LeftJoin<TAB>((a, ab) => ab.AId == a.Id)
                .LeftJoin<TAB, TB>((ab, b) => ab.BId == b.Id);

            var where = Db.Where<TA>(a => SqlStatement.In(a.Id, true, rtA.Select(s => s.Id)));

            IList<Tuple<TA, TAB?, TB?>>? result = await Db.RetrieveAsync<TA, TAB, TB>(from, where, null).ConfigureAwait(false);

            Assert.IsTrue(SerializeUtil.ToJson(rtA) == SerializeUtil.ToJson(result.Select(t => t.Item1).ToList()));
            Assert.IsTrue(SerializeUtil.ToJson(rtAB) == SerializeUtil.ToJson(result.Select(t => t.Item2).ToList()));
            Assert.IsTrue(SerializeUtil.ToJson(rtB) == SerializeUtil.ToJson(result.Select(t => t.Item3).ToList()));
        }

        [TestMethod]
        public async Task Test_JoinTest_ThreeTable()
        {
            await Test_JoinTest_ThreeTable_Core<MySql_Guid_JoinTest_A, MySql_Guid_JoinTest_B, MySql_Guid_JoinTest_AB>();
            await Test_JoinTest_ThreeTable_Core<MySql_Long_JoinTest_A, MySql_Long_JoinTest_B, MySql_Long_JoinTest_AB>();

            await Test_JoinTest_ThreeTable_Core<Sqlite_Guid_JoinTest_A, Sqlite_Guid_JoinTest_B, Sqlite_Guid_JoinTest_AB>();
            await Test_JoinTest_ThreeTable_Core<Sqlite_Long_JoinTest_A, Sqlite_Long_JoinTest_B, Sqlite_Long_JoinTest_AB>();
        }

        private async Task Test_JoinTest_TwoTable_Core<TA, TASub>() where TA : IJoinTest_A where TASub : IJoinTest_A_Sub
        {
            var rtA = await AddAndRetrieve<TA>(10);
            var rtASub = await AddAndRetrieve<TASub>(10, (i, aSub) =>
            {
                aSub.AId = rtA[i].Id;
            });

            var from = Db
                .From<TASub>()
                .LeftJoin<TA>((c, a) => c.AId == a.Id);

            var where = Db.Where<TASub>(aSub => SqlStatement.In(aSub.Id, true, rtASub.Select(s => s.Id)));

            IList<Tuple<TASub, TA?>>? result = await Db.RetrieveAsync<TASub, TA>(from, where, null).ConfigureAwait(false);

            Assert.IsTrue(SerializeUtil.ToJson(rtASub) == SerializeUtil.ToJson(result.Select(t => t.Item1).ToList()));
            Assert.IsTrue(SerializeUtil.ToJson(rtA) == SerializeUtil.ToJson(result.Select(t => t.Item2).ToList()));
        }

        [TestMethod]
        public async Task Test_JoinTest_TwoTable()
        {
            await Test_JoinTest_TwoTable_Core<MySql_Guid_JoinTest_A, MySql_Guid_JoinTest_A_Sub>();
            await Test_JoinTest_TwoTable_Core<MySql_Long_JoinTest_A, MySql_Long_JoinTest_A_Sub>();

            await Test_JoinTest_TwoTable_Core<Sqlite_Guid_JoinTest_A, Sqlite_Guid_JoinTest_A_Sub>();
            await Test_JoinTest_TwoTable_Core<Sqlite_Long_JoinTest_A, Sqlite_Long_JoinTest_A_Sub>();
        }
    }
}