using System;

namespace HB.FullStack.Client
{
    public delegate void ExceptionHandler(Exception ex, string message, string? caller);

    public enum ExceptionDisplayMode
    {
        Toast,
        Striking,
        StrikingAndConfirm,
    }
}