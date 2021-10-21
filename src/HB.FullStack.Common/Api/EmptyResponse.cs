namespace System.Net.Http
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class EmptyResponse
    {
        public static EmptyResponse Value { get; }

        static EmptyResponse()
        {
            Value = new EmptyResponse();
        }

        private EmptyResponse() { }
    }
}