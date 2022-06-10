namespace HB.FullStack.Client.Maui.Base
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