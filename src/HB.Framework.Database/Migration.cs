using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database
{
    public class Migration
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string SqlStatement { get; set; }

        public string TargetSchema { get; set; }

        public Migration(string targetSchema, int oldVersion, int newVersion, string sql)
        {
            if (targetSchema.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(targetSchema));
            }

            if (oldVersion < 1)
            {
                throw new ArgumentException("version should greater than 1.");
            }

            if (newVersion != oldVersion + 1)
            {
                throw new ArgumentException("Now days, you can only take 1 step further each time.");
            }

            if (sql.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(sql));
            }

            TargetSchema = targetSchema;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            SqlStatement = sql;
        }
    }
}
