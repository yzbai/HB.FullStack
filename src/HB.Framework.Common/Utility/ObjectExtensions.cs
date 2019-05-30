using System;
namespace System
{
    public static class ObjectExtensions
    {
        public static void RequireNotNull(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
