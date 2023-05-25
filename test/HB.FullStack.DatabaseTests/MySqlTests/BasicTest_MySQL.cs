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

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class BasicTest_MySQL : BaseTestClass
    {


        //NOTICE: 在重复update时，即值不发生改变。默认useAffectedRows=false，即update返回matched的数量。 而useAffectedRows=true，则返回真正发生过改变的数量。
        //应该保持useAffectedRows=false
        //[TestMethod]
        //[DataRow(true, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        //[DataRow(false, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        //[DataRow(null, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        public void TestMyS QL_UseAffectedRow_Test(bool? UseAffectedRows, string connectString)
        {
            if (UseAffectedRows.HasValue)
            {
                connectString += $"UseAffectedRows={UseAffectedRows};";
            }

            using MySqlConnection mySqlConnection = new MySqlConnection(connectString);
            mySqlConnection.Open();

            string guid = SecurityUtil.CreateUniqueToken();

            string insertCommandText = $"insert into tb_publisher(`Name`, `LastTime`, `Guid`) values('SSFS', 100, '{guid}')";

            using MySqlCommand insertCommand = new MySqlCommand(insertCommandText, mySqlConnection);

            insertCommand.ExecuteScalar();

            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Guid`='{guid}' ;";

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