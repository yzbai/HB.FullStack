using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace HB.Tools.JsonSchemaGen
{
    class TestOptions : IOptions<TestOptions>
    {
        public TestOptions Value => this;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            DbOptions options = new DbOptions();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var optionsTypes = Directory.GetFiles(path, "*.dll")
                .Select(Assembly.LoadFile)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.GetInterface("IOptions`1")!=null)
                .ToList();

            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(optionsTypes[0]);

            string text = schema.ToString();
        }
    }
}