using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Todo.Client.MobileApp
{
    public class TodoAppOptions : IOptions<TodoAppOptions>
    {
        public TodoAppOptions Value => this;

        public bool NeedLoginDefault { get; set; }
    }
}
