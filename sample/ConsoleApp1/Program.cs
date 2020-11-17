
using System;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ArgumentNullException argumentNull2 = new ArgumentNullException("yyyyyy is null");

                NotImplementedException e = new NotImplementedException("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", argumentNull2);

                ArgumentNullException argumentNull = new ArgumentNullException("xxx is null", e);


                argumentNull.Data["KeyArgument"] = "Null Argument";

                FrameworkException exception = new FrameworkException("a message from frameworkException", argumentNull);

                exception.Data["Key"] = "Some";

                throw exception;
            }
            catch (FrameworkException exception)
            {
                foreach (object item in exception.Data.Keys)
                {
                    Console.WriteLine($"{item.ToString()} : {exception.Data[item].ToString()}");
                }
                Console.WriteLine(exception);
            }
        }
    }
}
