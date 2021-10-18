using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api
{
	/// <summary>
	/// 从Xamarin.Forms中拷贝
	/// </summary>
	public class WrappedStream : Stream
	{
		readonly Stream _wrapped;
		IDisposable? _additionalDisposable;

		public WrappedStream(Stream wrapped) : this(wrapped, null)
		{
		}

		public WrappedStream(Stream wrapped, IDisposable? additionalDisposable)
		{
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
			_additionalDisposable = additionalDisposable;
		}

		public override bool CanRead
		{
			get { return _wrapped.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _wrapped.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _wrapped.CanWrite; }
		}

		public override long Length
		{
			get { return _wrapped.Length; }
		}

		public override long Position
		{
			get { return _wrapped.Position; }
			set { _wrapped.Position = value; }
		}

		public event EventHandler? Disposed;

		public override void Flush()
		{
			_wrapped.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _wrapped.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _wrapped.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_wrapped.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_wrapped.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			_wrapped.Dispose();
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
			return new WrappedStream(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), response);
		}
	}
}
