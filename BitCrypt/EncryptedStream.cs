namespace BitCrypt;

public class EncryptedStream : Stream
{
	private readonly Stream _baseStream;

	private readonly byte[] _key;
	private readonly byte[] _keyNum0;

	public override bool CanRead => _baseStream.CanRead;
	public override bool CanSeek => _baseStream.CanSeek;
	public override bool CanWrite => _baseStream.CanWrite;
	public override long Length => _baseStream.Length;

	public override long Position
	{
		get => _baseStream.Position;
		set => _baseStream.Position = value;
	}

	public EncryptedStream(Stream baseStream, byte[] key)
	{
		_key = key;
		_baseStream = baseStream;

		_keyNum0 = new byte[_key.Length];

		for (var i = 0; i < _key.Length; i++)
		{
			for (var j = 0; j < 8; j++)
			{
				if (((_key[i] >> j) & 1) == 0)
					_keyNum0[i]++;
			}
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _baseStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_baseStream.SetLength(value);
	}

	public override void Flush()
	{
		_baseStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		var startOffset = Position;
		var result = _baseStream.Read(buffer, offset, count);

		for (var i = 0; i < result; i++)
			buffer[offset + i] = Decrypt(buffer[offset + i], startOffset + i);

		return result;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		var startOffset = Position;
		var encrypted = new byte[count];

		for (var i = 0; i < count; i++)
			encrypted[i] = Encrypt(buffer[offset + i], startOffset + i);

		_baseStream.Write(encrypted);
	}

	private byte Encrypt(byte decrypted, long keyOffset)
	{
		var byteKey = _key[keyOffset % _key.Length];
		var byteKeyNum0 = _keyNum0[keyOffset % _keyNum0.Length];

		var encrypted = 0;
		var num0 = 0;
		var num1 = 0;

		for (var i = 0; i < 8; i++)
		{
			var keyBit = (byteKey >> i) & 1;
			encrypted |= ((((decrypted >> i) & 1) + keyBit) % 2) << (keyBit == 0 ? num0++ : byteKeyNum0 + num1++);
		}

		return (byte)encrypted;
	}

	private byte Decrypt(byte encrypted, long keyOffset)
	{
		var byteKey = _key[keyOffset % _key.Length];
		var byteKeyNum0 = _keyNum0[keyOffset % _keyNum0.Length];

		var decrypted = 0;
		var num0 = 0;
		var num1 = 0;

		for (var i = 0; i < 8; i++)
		{
			var keyBit = (byteKey >> i) & 1;
			decrypted |= ((((encrypted >> (keyBit == 0 ? num0++ : byteKeyNum0 + num1++)) + keyBit) % 2) & 1) << i;
		}

		return (byte)decrypted;
	}

	protected override void Dispose(bool disposing)
	{
		_baseStream.Dispose();

		base.Dispose(disposing);
	}
}
