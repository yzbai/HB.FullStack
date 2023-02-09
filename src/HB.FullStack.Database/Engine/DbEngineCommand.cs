using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{
    public class DbEngineCommand
    {

        public DbEngineCommand(string commandText, IList<KeyValuePair<string, object>>? parameters = null)
        {
            CommandText = commandText;
            Parameters = parameters;
        }

        public string CommandText { get; set; } = null!;


        public IList<KeyValuePair<string, object>>? Parameters { get; set; }
    }
}
