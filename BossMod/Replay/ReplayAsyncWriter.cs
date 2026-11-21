using System.Buffers;
using System.IO;
using System.Threading.Channels;

namespace BossMod;

internal sealed class BackgroundWriteStream : Stream
{
    private const int ChunkSize = 65536;

    private readonly Stream _underlying;
    private readonly Stream? _rawFileStream;
    private readonly Channel<(byte[]? Buffer, int Count, bool Flush)> _channel;
    private readonly Task _writerTask;
    private byte[]? _curBuffer;
    private int _curPos;
    private bool _disposed;
    private volatile Exception? _asyncError;

    public BackgroundWriteStream(Stream underlying, Stream? rawFileStream = null)
    {
        _underlying = underlying;
        _rawFileStream = rawFileStream;
        _curBuffer = ArrayPool<byte>.Shared.Rent(ChunkSize);
        _channel = Channel.CreateBounded<(byte[]? Buffer, int Count, bool Flush)>(new BoundedChannelOptions(64)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });
        _writerTask = Task.Run(ConsumerLoop);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => !_disposed;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfFailed();

        while (!buffer.IsEmpty)
        {
            _curBuffer ??= ArrayPool<byte>.Shared.Rent(ChunkSize);
            var space = _curBuffer.Length - _curPos;
            if (space == 0)
            {
                EnqueueCurrentBuffer(false);
                _curBuffer = ArrayPool<byte>.Shared.Rent(ChunkSize);
                space = _curBuffer.Length;
            }
            var toCopy = Math.Min(space, buffer.Length);
            buffer[..toCopy].CopyTo(_curBuffer.AsSpan(_curPos));
            _curPos += toCopy;
            buffer = buffer[toCopy..];
        }
    }

    public override void WriteByte(byte value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfFailed();

        _curBuffer ??= ArrayPool<byte>.Shared.Rent(ChunkSize);
        if (_curPos == _curBuffer.Length)
        {
            EnqueueCurrentBuffer(false);
            _curBuffer = ArrayPool<byte>.Shared.Rent(ChunkSize);
        }
        _curBuffer[_curPos++] = value;
    }

    public override void Flush()
    {
        if (_disposed)
        {
            return;
        }
        ThrowIfFailed();
        EnqueueCurrentBuffer(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                try
                {
                    try
                    {
                        if (_asyncError == null && _curBuffer != null && _curPos > 0)
                        {
                            EnqueueCurrentBuffer(false);
                        }
                    }
                    finally
                    {
                        if (_curBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(_curBuffer);
                            _curBuffer = null;
                        }

                        _channel.Writer.TryComplete();
                        _writerTask.GetAwaiter().GetResult();
                    }
                }
                finally
                {
                    try
                    {
                        _underlying.Dispose();
                    }
                    finally
                    {
                        if (_rawFileStream != null && !ReferenceEquals(_rawFileStream, _underlying))
                        {
                            _rawFileStream.Dispose();
                        }
                    }
                }
            }
        }
        base.Dispose(disposing);
    }

    private void EnqueueCurrentBuffer(bool flush)
    {
        var buf = _curBuffer;
        var count = _curPos;
        if (buf == null && !flush)
        {
            return;
        }

        _curBuffer = null;
        _curPos = 0;

        if (!_channel.Writer.TryWrite((buf, count, flush)))
        {
            if (!_channel.Writer.WriteAsync((buf, count, flush)).AsTask().Wait(TimeSpan.FromSeconds(10)))
            {
                if (buf != null)
                {
                    ArrayPool<byte>.Shared.Return(buf);
                }
                ThrowIfFailed();
                throw new IOException("Timed out queuing replay write chunk");
            }
        }
        ThrowIfFailed();
    }

    private void ThrowIfFailed()
    {
        if (_asyncError != null)
        {
            throw new IOException("Replay background writer failed", _asyncError);
        }
    }

    private async Task ConsumerLoop()
    {
        try
        {
            var reader = _channel.Reader;
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var item))
                {
                    var buf = item.Buffer;
                    try
                    {
                        if (buf != null && item.Count > 0)
                        {
                            _underlying.Write(buf, 0, item.Count);
                        }
                        if (item.Flush)
                        {
                            _underlying.Flush();
                            if (_rawFileStream != null && !ReferenceEquals(_rawFileStream, _underlying))
                            {
                                _rawFileStream.Flush();
                            }
                        }
                    }
                    finally
                    {
                        if (buf != null)
                        {
                            ArrayPool<byte>.Shared.Return(buf);
                        }
                    }
                }
            }
            _underlying.Flush();
            if (_rawFileStream != null && !ReferenceEquals(_rawFileStream, _underlying))
            {
                _rawFileStream.Flush();
            }
        }
        catch (Exception ex)
        {
            _asyncError = ex;
            Service.Log($"[ReplayRecorder] Background write failed: {ex}");
            while (_channel.Reader.TryRead(out var leftover))
            {
                if (leftover.Buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(leftover.Buffer);
                }
            }
        }
    }
}
