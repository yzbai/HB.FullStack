namespace HB.FullStack.Common.Api
{
    public class HttpEndpointSettings
    {
        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = "Bearer";

        public HttpMethodOverrideMode HttpMethodOverrideMode { get; set; } = HttpMethodOverrideMode.None;
    }
}