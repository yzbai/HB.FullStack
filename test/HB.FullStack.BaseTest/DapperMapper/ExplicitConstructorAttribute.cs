using System;

namespace HB.FullStack.BaseTest.DapperMapper
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class ExplicitConstructorAttribute : Attribute
    {
    }
}
