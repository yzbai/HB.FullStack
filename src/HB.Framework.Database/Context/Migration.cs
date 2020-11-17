#nullable enable

using HB.Framework.Database.Properties;
using System;

namespace HB.Framework.Database
{
    public class Migration
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string SqlStatement { get; set; }

        public string TargetSchema { get; set; }

        /// <exception cref="System.ArgumentException"></exception>
        public Migration(string targetSchema, int oldVersion, int newVersion, string sql)
        {
            //if (targetSchema.IsNullOrEmpty())
            //{
            //    throw new ArgumentNullException(nameof(targetSchema));
            //}

            if (oldVersion < 1)
            {
                throw new ArgumentException(Resources.MigrateOldVersionErrorMessage);
            }

            if (newVersion != oldVersion + 1)
            {
                throw new ArgumentException(Resources.MigrateVersionStepErrorMessage);
            }

            //if (sql.IsNullOrEmpty())
            //{
            //    throw new ArgumentNullException(nameof(sql));
            //}

            TargetSchema = targetSchema;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            SqlStatement = sql;
        }
    }
}
