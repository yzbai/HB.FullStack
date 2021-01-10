using System;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LocalDataTimeoutAttribute : Attribute
    {
        public TimeSpan ExpiryTime { get; set; }

        public LocalDataTimeoutAttribute(int expiryMinutes)
        {
            ExpiryTime = TimeSpan.FromMinutes(expiryMinutes);
        }
    }
}