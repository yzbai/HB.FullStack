using System;
namespace System
{
    public static class ObjectExtensions
    {
        public static void RequireNotNullOrEmpty(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
