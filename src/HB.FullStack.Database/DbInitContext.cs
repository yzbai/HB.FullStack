using System.Collections.Generic;

namespace HB.FullStack.Database
{
    public class DbInitContext
    {
        public string DbSchemaName { get; set; } = null!;

        public string? ConnectionString { get; set; }

        public IList<string>? SlaveConnectionStrings { get; set; }

        public IList<Migration>? Migrations { get; set; }


    }
}