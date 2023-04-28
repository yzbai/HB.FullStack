/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

namespace HB.FullStack.Client.MauiLib.Base
{
    public delegate void ExceptionHandler(Exception ex, string message, string? caller);

    public enum ExceptionDisplayMode
    {
        Toast,
        Striking,
        StrikingAndConfirm,
    }
}