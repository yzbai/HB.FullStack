using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.CommonTests
{
    internal class SimpleAppOptions : IOptions<SimpleAppOptions>
    {
        public string Name { get; set; } = default!;

        public int Age { get; set;}

        public Uri Url { get; set; } = default!;

        public IList<InnerSettings>? Inners { get; private set; } = new List<InnerSettings> { new InnerSettings {  Name = "xxxxxxxx"} };


        public SimpleAppOptions Value => this;
    }

    class InnerSettings
    {
        public string? Name { get; set; }
    }
}
