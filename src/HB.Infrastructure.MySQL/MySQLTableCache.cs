using System;
using System.Collections;
using System.Data;
using MySql.Data.MySqlClient;

namespace HB.Infrastructure.MySQL
{
    public static class MySQLTableCache
    {

        #region 构造函数 和 私有

        #endregion

        public static DataTable CreateEmptyDataTable(string connectString, string tableName)
        {
            throw new NotImplementedException();
            //string key = connectString + ":" + tableName;

            //if (!cachedTable.ContainsKey(key))
            //{
            //    using (MySqlConnection conn = new MySqlConnection(connectString))
            //    {
            //        conn.Open();

            //        DataTable dataTable = new DataTable();



            //        using (MySqlDataAdapter adapter = new MySqlDataAdapter(SELECT_SQL + tableName, conn))
            //        {
            //            adapter.Fill(dataTable);    
            //        }

            //        cachedTable[key] = dataTable;
            //    }
            //}

            //return ((DataTable)cachedTable[key]).Copy();
        }
    }
}
