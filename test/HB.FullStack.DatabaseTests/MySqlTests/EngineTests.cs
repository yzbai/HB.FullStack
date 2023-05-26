global using HB.FullStack.BaseTest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MySqlConnector;
using HB.FullStack.BaseTest.Models;
using HB.FullStack.BaseTest.DapperMapper;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using HB.FullStack.Database.Config;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class EngineTests : DatabaseTestClass
    {
        private async Task Test_Repeate_Update_Return_1_Core<T>() where T : IDbModel
        {
            var dbModel = Mocker.Mock<T>(1).First();

            await Db.AddAsync(dbModel, "tester", null);


            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            var parameters = new List<KeyValuePair<string, object>>()
                .AddParameter(modelDef.PrimaryKeyPropertyDef, modelDef.PrimaryKeyPropertyDef.GetValueFrom(dbModel), null, 0);

            var command = new DbEngineCommand(
                $"update {modelDef.DbTableReservedName} set LastUser ='Update_xxx' where Id = {parameters[0].Key}",
                parameters);

            int rt = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);
            int rt2 = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);
            int rt3 = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);

            Assert.IsTrue(1 == rt);

            Assert.IsTrue(1 == rt2);

            Assert.IsTrue(1 == rt3);
        }

        /// <summary>
        /// //NOTICE: 在sqlite下，重复update，返回1.即matched
        /// //NOTICE: 在mysql下，重复update，返回1，即mactched
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Repeate_Update_Return_1()
        {
            await Test_Repeate_Update_Return_1_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Repeate_Update_Return_1_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Mult_SQL_Return_With_Reader_Core<T>() where T : IBookModel
        {
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            var parameters = new List<KeyValuePair<string, object>>().AddParameter(modelDef.PrimaryKeyPropertyDef, modelDef.PrimaryKeyPropertyDef.GetValueFrom(book), null, 0);

            string sql = @$"
update {modelDef.DbTableReservedName} set LastUser='TTTgdTTTEEST' where Id = {parameters[0].Key};
select count(1) from {modelDef.DbTableReservedName} where Id = {parameters[0].Key};
";
            var command = new DbEngineCommand(sql, parameters);

            using IDataReader reader = await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command);

            List<string?> rt = new List<string?>();

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rt.Add(reader.GetValue(i)?.ToString());
                }
            }

            Assert.AreEqual(rt.Count, 1);
            Assert.AreNotEqual(rt.Count, 1);
        }


        /// <summary>
        /// //NOTICE: Mysql执行多条语句的时候，ExecuteCommandReader只返回最后一个结果。
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Mult_SQL_Return_With_Reader()
        {
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();
        }


        //NOTICE: 在重复update时，即值不发生改变。默认useAffectedRows=false，即update返回matched的数量。 而useAffectedRows=true，则返回真正发生过改变的数量。
        //应该保持useAffectedRows=false
        [TestMethod]
        public async void Test_MySQL_UseAffectedRow_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            if (modelDef.EngineType != DbEngineType.MySQL)
            {
                return;
            }

            DbConnectionStringBuilder connectionBuilder = new DbConnectionStringBuilder();
            connectionBuilder.ConnectionString = modelDef.MasterConnectionString.ToString();

            connectionBuilder["UseAffectedRows"] = false;

            var falseUseAffectedRowsConnection = new ConnectionString(connectionBuilder.ConnectionString);

            connectionBuilder["UseAffectedRows"] = true;

            var trueUseAffectedRowConnection = new ConnectionString(connectionBuilder.ConnectionString);

            connectionBuilder.Remove("UserAffectedRows");

            var noneUseAffectedRowConnection = new ConnectionString(connectionBuilder.ConnectionString);


            var model = (await AddAndRetrieve<T>(1)).First();

            string commandText = $"update {modelDef.DbTableReservedName} set LastUser ={modelDef.LastUserPropertyDef.DbParameterizedName} WHERE Id ={modelDef.PrimaryKeyPropertyDef.DbParameterizedName};";

            var engine = modelDef.Engine;

            var selectRowCountCommand = new DbEngineCommand("select row_count()");

            //UseAffectedRows = false
            var getParameters =()=> new List<KeyValuePair<string, object>>()
                .AddParameter(modelDef.PrimaryKeyPropertyDef, model.Id)
                .AddParameter(modelDef.LastUserPropertyDef, SecurityUtil.CreateRandomString(5));

            var falseCommand = new DbEngineCommand(commandText, getParameters());

            int falseRt1 = await engine.ExecuteCommandNonQueryAsync(falseUseAffectedRowsConnection, falseCommand);
            var falseCount1 = await engine.ExecuteCommandScalarAsync(falseUseAffectedRowsConnection, selectRowCountCommand);
            int falseRt2 = await engine.ExecuteCommandNonQueryAsync(falseUseAffectedRowsConnection, falseCommand);
            var falseCount2 = await engine.ExecuteCommandScalarAsync(falseUseAffectedRowsConnection, selectRowCountCommand);

            Assert.IsTrue(falseRt1 == 1);
            Assert.IsTrue(falseRt2 == 1);
            Assert.IsTrue(Convert.ToInt64(falseCount1) == 1);
            Assert.IsTrue(Convert.ToInt64(falseCount2) == 1);

            int fa

            using MySqlCommand mySqlCommand1 = new MySqlCommand(commandText, mySqlConnection);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand rowCountCommand1 = new MySqlCommand("select row_count()", mySqlConnection);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using MySqlCommand mySqlCommand2 = new MySqlCommand(commandText, mySqlConnection);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand rowCountCommand2 = new MySqlCommand("select row_count()", mySqlConnection);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            if (UseAffectedRows.HasValue && UseAffectedRows.Value) //真正改变的行数
            {
                Assert.AreNotEqual(rt1, rt2);
                Assert.AreNotEqual(rowCount1, rowCount2);
            }
            else //found_rows 找到的行数  by default in mysql
            {
                Assert.AreEqual(rt1, rt2);
                Assert.AreEqual(rowCount1, rowCount2);
            }
        }

        [TestMethod]
        public void TestSQLite_Changes_Test()
        {
            using SqliteConnection conn = new SqliteConnection(SqliteConnectionString);
            conn.Open();

            long id = new Random().NextInt64(long.MaxValue);
            long timestamp = TimeUtil.Timestamp;

            string insertCommandText = $"insert into tb_publisher(`Name`, `Id`, `Timestamp`) values('FSFSF', '{id}', {timestamp})";

            using SqliteCommand insertCommand = new SqliteCommand(insertCommandText, conn);

            insertCommand.ExecuteScalar();

            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Timestamp`={timestamp} WHERE `Id`='{id}' ;";

            using SqliteCommand mySqlCommand1 = new SqliteCommand(commandText, conn);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand1 = new SqliteCommand("select changes()", conn);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using SqliteCommand mySqlCommand2 = new SqliteCommand(commandText, conn);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand2 = new SqliteCommand("select changes()", conn);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            Assert.AreEqual(rt1, rt2, rowCount1.ToString(), rowCount2.ToString());
        }
    }
}