using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HB.PresentFish.Tools
{
    public class SQLCreator
    {
        private ISQLBuilder _sqlBuilder;

        public SQLCreator(ISQLBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;

            CreateSqlFromAssembly("HB.Framework.Identity");
            CreateSqlFromAssembly("HB.Framework.AuthorizationServer");
        }

        private void CreateSqlFromAssembly(string assemblyName)
        {
            StringBuilder stringBuilder = new StringBuilder();

            Assembly assembly = Assembly.Load(assemblyName);

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(DatabaseEntity)))
                {
                    stringBuilder.AppendLine(_sqlBuilder.GetCreateStatement(type, false));
                    stringBuilder.AppendLine();
                }
            }

            using (FileStream fileStream = new FileStream($"{assemblyName}.txt", FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(stringBuilder.ToString());
                }
            }
        }
    }
}
