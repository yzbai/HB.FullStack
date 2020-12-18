using HB.FullStack.DatabaseTests.Data;

using Microsoft.Data.Sqlite;

using MySqlConnector;

using System;

using Xunit;

namespace HB.FullStack.DatabaseTests
{
    /// <summary>
    // 
    /// </summary>
    public class UseAffectedRowsTest
    {
        /// <summary>
        /// TestUseAffectedRow_When_True_Test
        /// </summary>
        
        [Theory]
        [InlineData(true, "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None;")]
        [InlineData(false, "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None;")]
        [InlineData(null, "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None;")]
        public void TestMySQL_UseAffectedRow_Test(bool? UseAffectedRows, string connectString)
        {
            if (UseAffectedRows.HasValue)
            {
                connectString = connectString + $"UseAffectedRows={UseAffectedRows};";
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
                Assert.NotEqual(rt1, rt2);
                Assert.NotEqual(rowCount1, rowCount2);
            }
            else //found_rows 找到的行数  by default in mysql
            {
                Assert.Equal(rt1, rt2);
                Assert.Equal(rowCount1, rowCount2);
            }
        }

        [Fact]
        public void TestSQLite_Changes_Test()
        {
            string connectString = $"Data Source=sqlite_test2.db";
            using SqliteConnection conn = new SqliteConnection(connectString);
            conn.Open();

            string guid = SecurityUtil.CreateUniqueToken();

            string insertCommandText = $"insert into tb_publisher(`Name`, `LastTime`, `Guid`, `Version`) values('SSFS', 100, '{guid}', 1)";

            using SqliteCommand insertCommand = new SqliteCommand(insertCommandText, conn);

            insertCommand.ExecuteScalar();


            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Guid`='{guid}' ;";

            using SqliteCommand mySqlCommand1 = new SqliteCommand(commandText, conn);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand1 = new SqliteCommand("select changes()", conn);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using SqliteCommand mySqlCommand2 = new SqliteCommand(commandText, conn);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand2 = new SqliteCommand("select changes()", conn);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            Assert.Equal(rt1, rt2);
            Assert.Equal(rowCount1, rowCount2);
        }
    }
}
