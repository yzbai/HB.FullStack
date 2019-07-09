using HB.Framework.Database;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.SQLite.Test
{
    [TestCaseOrderer("HB.Framework.Database.Test.TestCaseOrdererByTestName", "HB.Framework.Database.Test")]
    public class MutipleTableTest
    {
        private IDatabase db;
        private ITestOutputHelper output; 

        public MutipleTableTest(ITestOutputHelper testOutputHelper)
        {
            output = testOutputHelper;

            db = GetDatabase();

            AddSomeData();

        }

        private void AddSomeData()
        {
            A a1 = new A { Name = "a1" };
            A a2 = new A { Name = "a2" };
            A a3 = new A { Name = "a3" };

            B b1 = new B { Name = "b1" };
            B b2 = new B { Name = "b2" };

            AB a1b1 = new AB { AId = a1.Guid, BId = b1.Guid };
            AB a1b2 = new AB { AId = a1.Guid, BId = b2.Guid };

            AB a2b1 = new AB { AId = a2.Guid, BId = b1.Guid };
            AB a3b2 = new AB { AId = a3.Guid, BId = b2.Guid };

            C c1 = new C { AId = a1.Guid };
            C c2 = new C { AId = a2.Guid };
            C c3 = new C { AId = a3.Guid };
            C c4 = new C { AId = a1.Guid };
            C c5 = new C { AId = a2.Guid };
            C c6 = new C { AId = a3.Guid };

            db.Add(a1, null);
            db.Add(a2, null);
            db.Add(a3, null);

            db.Add(b1, null);
            db.Add(b2, null);

            db.Add(a1b1, null);
            db.Add(a1b2, null);
            db.Add(a2b1, null);
            db.Add(a3b2, null);

            db.Add(c1, null);
            db.Add(c2, null);
            db.Add(c3, null);
            db.Add(c4, null);
            db.Add(c5, null);
            db.Add(c6, null);
        }

        private IDatabase GetDatabase()
        {
            SQLiteOptions sqliteOptions = new SQLiteOptions();

            sqliteOptions.DatabaseSettings.Version = 1;
            sqliteOptions.Schemas.Add(new SchemaInfo {
                SchemaName = "test.db",
                IsMaster = true,
                ConnectionString = "Data Source=c:\\Share\\test.db;"
            });

            IDatabase database = new DatabaseBuilder(new SQLiteBuilder(sqliteOptions).Build()).Build();

            database.Initialize();

            return database;
        }

        [Fact]
        public void Test_1_ThreeTable_JoinTest()
        {
            var from = db
                .From<A>()
                .LeftJoin<AB>((a, ab) => ab.AId == a.Guid)
                .LeftJoin<AB, B>((ab, b) => ab.BId == b.Guid);


            try
            {
                var result = db.Retrieve<A, AB, B>(from, db.Where<A>(), null);
                Assert.True(result.Count > 0);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);

                throw ex;
            }

            
        }

        [Fact]
        public void Test_2_TwoTable_JoinTest()
        {
            var from = db
                .From<C>()
                .LeftJoin<A>((c, a) => c.AId == a.Guid);


            try
            {
                var result = db.Retrieve<C, A>(from, db.Where<C>(), null);
                Assert.True(result.Count > 0);
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);

                throw ex;
            }
        }
    }

    public class A : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty]
        public string Name { get; set; }
    }

    public class B : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty]
        public string Name { get; set; }
    }

    public class AB : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty]
        public string AId { get; set; }

        [EntityProperty]
        public string BId { get; set; }
    }

    public class C : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [EntityProperty]
        public string Name { get; set; }

        [EntityProperty]
        public string AId { get; set; }
    }

}
