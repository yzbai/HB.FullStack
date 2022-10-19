using System.Collections.Generic;

namespace HB.FullStack.CommonTests
{
    public class TestModel
    {
        public string? Name { get; set; }

        public IEnumerable<string> Values { get; init; } = new List<string>();
    }
}