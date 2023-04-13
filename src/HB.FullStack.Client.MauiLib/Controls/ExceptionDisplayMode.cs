using System;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public delegate void ExceptionHandler(Exception ex, string message, string? caller);

    public enum ExceptionDisplayMode
    {
        Toast,
        Striking,
        StrikingAndConfirm,
    }
}