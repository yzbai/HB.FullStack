using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using OrmBenchmark.Core;
using System.Dynamic;
using MySql.Data.MySqlClient;

namespace OrmBenchmark.Ado
{
    public class PureAdoExecuter : IOrmExecuter
    {
        MySqlConnection conn;

        public string Name
        {
            get
            {
                return "ADO (Pure)";
            }
        }

        public void Init(string connectionStrong)
        {
            conn = new MySqlConnection(connectionStrong);
            conn.Open();
        }
        public IPost GetItemAsObject(int Id)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from Posts where Id = @Id";
            var idParam = cmd.Parameters.Add("@Id", MySqlDbType.Int64);
            idParam.Value = Id;

            Post obj;
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                obj = new Post
                {
                    Id = reader.GetInt64(0),
                    Text = reader.GetString(5),
                    CreationDate = reader.GetInt64(6),
                    LastChangeDate = reader.GetInt64(7),
                    Counter1 = reader.GetInt32(8),
                    Counter2 = reader.GetInt32(9),
                    Counter3 = reader.GetInt32(10),
                    Counter4 = reader.GetInt32(11),
                    Counter5 = reader.GetInt32(12),
                    Counter6 = reader.GetInt32(13),
                    Counter7 = reader.GetInt32(14),
                    Counter8 = reader.GetInt32(15),
                    Counter9 = reader.GetInt32(16),
                };
            }

            return obj;
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            return null;

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from Posts where Id = @Id";
            var idParam = cmd.Parameters.Add("@Id", MySqlDbType.Int64);
            idParam.Value = Id;

            dynamic obj;
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                obj = new
                {
                    Id = reader.GetInt64(0),
                    Text = reader.GetString(5),
                    CreationDate = reader.GetInt64(6),
                    LastChangeDate = reader.GetInt64(7),
                    Counter1 = reader.GetInt32(8),
                    Counter2 = reader.GetInt32(9),
                    Counter3 = reader.GetInt32(10),
                    Counter4 = reader.GetInt32(11),
                    Counter5 = reader.GetInt32(12),
                    Counter6 = reader.GetInt32(13),
                    Counter7 = reader.GetInt32(14),
                    Counter8 = reader.GetInt32(15),
                    Counter9 = reader.GetInt32(16),
                };
            }

            return obj;
        }

        public IEnumerable<IPost> GetAllItemsAsObject()
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from Posts";

            List<IPost> list = new List<IPost>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Post obj = new Post
                    {
                        Id = reader.GetInt64(0),
                        Text = reader.GetString(5),
                        CreationDate = reader.GetInt64(6),
                        LastChangeDate = reader.GetInt64(7),
                        Counter1 = reader.GetInt32(8),
                        Counter2 = reader.GetInt32(9),
                        Counter3 = reader.GetInt32(10),
                        Counter4 = reader.GetInt32(11),
                        Counter5 = reader.GetInt32(12),
                        Counter6 = reader.GetInt32(13),
                        Counter7 = reader.GetInt32(14),
                        Counter8 = reader.GetInt32(15),
                        Counter9 = reader.GetInt32(16),
                    };

                    list.Add(obj);
                }
            }

            return list;
        }

        public IEnumerable<dynamic> GetAllItemsAsDynamic()
        {
            return null;

#if NETFULL
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from Posts";

            List<dynamic> list = new List<dynamic>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dynamic obj = new
                    {
                        Id = reader.GetInt32(0),
                        Text = reader.GetNullableString(1),
                        CreationDate = reader.GetDateTime(2),
                        LastChangeDate = reader.GetDateTime(3),
                        Counter1 = reader.GetNullableValue<int>(4),
                        Counter2 = reader.GetNullableValue<int>(5),
                        Counter3 = reader.GetNullableValue<int>(6),
                        Counter4 = reader.GetNullableValue<int>(7),
                        Counter5 = reader.GetNullableValue<int>(8),
                        Counter6 = reader.GetNullableValue<int>(9),
                        Counter7 = reader.GetNullableValue<int>(10),
                        Counter8 = reader.GetNullableValue<int>(11),
                        Counter9 = reader.GetNullableValue<int>(12),
                    };

                    list.Add(obj);
                }
            }

            return list;
#else
            return null;
#endif
        }

        public void Finish()
        {
            conn.Close();
        }

    }
}
