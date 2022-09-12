
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Test
{
    public class TestHttpServer : IDisposable
    {
        readonly HttpListener _listener;
        readonly IList<TestRequestHandler> _requestHandlers;
        readonly object _requestHandlersLock = new();
        readonly CancellationTokenSource _cts = new();

        public int Port { get; }

        public TestHttpServer(
            int port,
            string url,
            HttpMethod httpMethod,
            Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>?> handlerAction,
            string hostName = "localhost"
        )
            : this(port, new List<TestRequestHandler> { new(url, httpMethod, handlerAction) }, hostName) { }

        /// <summary>
        /// 生成并启动TestHttpServer
        /// </summary>
        /// <param name="port">0表示使用随机端口</param>
        /// <param name="handlers"></param>
        /// <param name="hostName"></param>
        public TestHttpServer(
            int port,
            IList<TestRequestHandler> handlers,
            string hostName = "localhost"
        )
        {
            _requestHandlers = handlers;

            Port = port > 0 ? port : GetRandomUnusedPort();

            //create and start listener
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{hostName}:{Port}/");
            _listener.Start();

            _ = Task.Run(() => HandleRequestsAsync(_cts.Token));
        }

        static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        async Task HandleRequestsAsync(CancellationToken cancellationToken)
        {
            try
            {
                //listen for all requests
                while (_listener.IsListening && !cancellationToken.IsCancellationRequested)
                {
                    //get the request
                    var context = await _listener.GetContextAsync().ConfigureAwait(true);

                    try
                    {
                        Dictionary<string, string>? parameters = null;
                        TestRequestHandler? handler;

                        lock (_requestHandlersLock)
                        {
                            handler = _requestHandlers.FirstOrDefault(
                                h => h.TryMatchUrl(context.Request.RawUrl, context.Request.HttpMethod, out parameters)
                            );
                        }

                        string? responseString = null;

                        if (handler != null)
                        {
                            //add the query string parameters to the pre-defined url parameters that were set from MatchesUrl()
                            foreach (string? qsParamName in context.Request.QueryString.AllKeys)
                                parameters![qsParamName!] = context.Request.QueryString[qsParamName!]!;

#pragma warning disable CA1031 // Do not catch general exception types
                            try
                            {
                                handler.HandlerAction(context.Request, context.Response, parameters);
                            }
                            catch (Exception ex)
                            {
                                responseString = $"Exception in handler: {ex.Message}";
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
#pragma warning restore CA1031 // Do not catch general exception types
                        }
                        else
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.StatusCode = 404;
                            responseString = "No handler provided for URL: " + context.Request.RawUrl;
                        }

                        //context.Request.ClearContent();

                        //send the response, if there is not (if responseString is null, then the handler method should have manually set the output stream)
                        if (responseString != null)
                        {
                            var buffer = Encoding.UTF8.GetBytes(responseString);
                            context.Response.ContentLength64 += buffer.Length;
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                        }
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                        context.Response.Close();
                    }
                }
            }
            catch (HttpListenerException ex)
            {
                //when the listener is stopped, it will throw an exception for being cancelled, so just ignore it
                if (ex.Message != "The I/O operation has been aborted because of either a thread exit or an application request")
                    throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _listener.IsListening)
            {
                _listener.Stop();
            }

            _listener.Close();
            _cts.Dispose();
        }
    }
}
