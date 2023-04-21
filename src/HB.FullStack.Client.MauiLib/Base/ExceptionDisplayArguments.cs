/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

namespace HB.FullStack.Client.MauiLib.Base
{
    public class ExceptionDisplayArguments
    {
        public string Message { get; set; }

        public ExceptionDisplayMode DisplayMode { get; set; }

        public ExceptionDisplayArguments(string message, ExceptionDisplayMode displayMode)
        {
            Message = message;
            DisplayMode = displayMode;
        }
    }
}