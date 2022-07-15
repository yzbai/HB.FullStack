using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Utils
{
    /// <summary>
    /// 工具类：提供在dispose stream的同时，dipose附加。往往是stream的源头或者产生者
    /// 比如HttpResponse产出Stream，在dispose stream后才能dispose httpResponse
    /// </summary>
    public class WrappedStream : Stream
    {
        private readonly Stream _stream;
        private IDisposable? _additionalDisposable;

        public WrappedStream(Stream stream, IDisposable? additionalDisposable)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _additionalDisposable = additionalDisposable;
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public event EventHandler? Disposed;

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            _stream.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
            _additionalDisposable?.Dispose();
            _additionalDisposable = null;

            base.Dispose(disposing);
        }

        public static async Task<Stream?> GetStreamAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                //Internals.Log.Warning("HTTP Request", $"Could not retrieve {uri}, status code {response.StatusCode}");
                return null;
            }

            // the HttpResponseMessage needs to be disposed of after the calling code is done with the stream
            // otherwise the stream may get disposed before the caller can use it

#if NET5_0_OR_GREATER
            return new WrappedStream(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), response);
#elif NETSTANDARD2_1
            return new WrappedStream(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), response);
#elif NETSTANDARD2_0
            return new WrappedStream(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), response);
#endif
        }
    }
}