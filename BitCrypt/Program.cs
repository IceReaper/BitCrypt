using BitCrypt;

if (args.Length != 4)
	Console.WriteLine("Usage: BitCrypt.exe [--decrypt|--encrypt] <key file> <input file> <output file>");
else
{
	Stream input;
	Stream output;

	var key = File.ReadAllBytes(args[1]);

	switch (args[0])
	{
		case "--decrypt":
		{
			input = new EncryptedStream(File.Open(args[2], FileMode.Open, FileAccess.Read), key);
			output = File.Open(args[3], FileMode.Create, FileAccess.Write);

			break;
		}

		case "--encrypt":
		{
			input = File.Open(args[2], FileMode.Open, FileAccess.Read);
			output = new EncryptedStream(File.Open(args[3], FileMode.Create, FileAccess.Write), key);

			break;
		}

		default:
			return;
	}

	input.CopyTo(output);
	input.Dispose();
	output.Dispose();
}
