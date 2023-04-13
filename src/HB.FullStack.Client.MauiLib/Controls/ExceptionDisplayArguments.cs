namespace HB.FullStack.Client.MauiLib.Controls
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