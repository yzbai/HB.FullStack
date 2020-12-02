using System.Text.Json.Serialization;

namespace System.Tests
{
    class NumberTestCls
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(IntToStringConverter))]
        public int Number { get; set; }

        [JsonConverter(typeof(DoubleToStringConverter))]
        public double Price { get; set; }
    }

   

}