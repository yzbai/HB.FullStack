using MySqlConnector;
using System;
using Xunit;

namespace HB.Framework.DatabaseTests
{
    /// <summary>
    // 
    /// </summary>
    public class MySQLUseAffectedRowsTest : IClassFixture<ServiceFixture>
    {
        /// <summary>
        /// TestUseAffectedRow_When_True_Test
        /// </summary>
        /// <exception cref="Xunit.Sdk.NotEqualException">Ignore.</exception>
        [Fact]
        public void TestUseAffectedRow_When_True_Test()
        {
            string connectString = "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None;UseAffectedRows=true";

            using MySqlConnection mySqlConnection = new MySqlConnection(connectString);
            mySqlConnection.Open();

            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Id`=1 ;";

            using MySqlCommand mySqlCommand1 = new MySqlCommand(commandText, mySqlConnection);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand mySqlCommand2 = new MySqlCommand(commandText, mySqlConnection);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            Assert.NotEqual(rt1, rt2);
        }

        /// <summary>
        /// TestUseAffectedRow_When_False_Test
        /// </summary>
        /// <exception cref="Xunit.Sdk.EqualException">Ignore.</exception>
        [Fact]
        public void TestUseAffectedRow_When_False_Test()
        {
            string connectString = "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None;UseAffectedRows=false";

            using MySqlConnection mySqlConnection = new MySqlConnection(connectString);
            mySqlConnection.Open();

            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Id`=1 ;";

            using MySqlCommand mySqlCommand1 = new MySqlCommand(commandText, mySqlConnection);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand mySqlCommand2 = new MySqlCommand(commandText, mySqlConnection);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            Assert.Equal(rt1, rt2);
        }
    }
}
