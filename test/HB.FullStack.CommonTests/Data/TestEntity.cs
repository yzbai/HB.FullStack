using System.Collections.Generic;

namespace HB.FullStack.CommonTests
{
    public class TestEntity
    {
        public string? Name { get; set; }

        public IEnumerable<string> Values { get; init; } = new List<string>();
    }
}