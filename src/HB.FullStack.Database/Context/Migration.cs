#nullable enable

using System;

namespace HB.FullStack.Database
{
    public class Migration
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string SqlStatement { get; set; }

        public string TargetSchema { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="targetSchema"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        /// <param name="sql"></param>
        /// <exception cref="DatabaseException"></exception>
        public Migration(string targetSchema, int oldVersion, int newVersion, string sql)
        {
            //if (targetSchema.IsNullOrEmpty())
            //{
            //    throw new ArgumentNullException(nameof(targetSchema));
            //}

            if (oldVersion < 1)
            {
                throw new DatabaseException(DatabaseErrorCode.MigrateOldVersionErrorMessage);
            }

            if (newVersion != oldVersion + 1)
            {
                throw new DatabaseException(DatabaseErrorCode.MigrateVersionStepErrorMessage);
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