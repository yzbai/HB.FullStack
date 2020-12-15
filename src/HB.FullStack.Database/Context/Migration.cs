#nullable enable

using System;

using HB.FullStack.Database.Properties;

namespace HB.FullStack.Database
{
    public class Migration
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string SqlStatement { get; set; }

        public string TargetSchema { get; set; }

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