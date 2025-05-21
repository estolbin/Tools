if (args.Length == 0)
{
    Console.WriteLine("Usage: touch [options] <file1> [file2 ...]");
    
    return;
}

bool noCreate = false;
DateTime? specifiedTime = null;

for (int i = 0; i < args.Length; i++)
{
    string arg = args[i];

    if (arg.StartsWith("-"))
    {
        if (arg == "--no-create")
        {
            noCreate = true;
        }
        else if (arg.StartsWith("--time="))
        {
            if (DateTime.TryParse(arg.Substring(7), out var parsedTime))
            {
                specifiedTime = parsedTime;
            }
            else
            {
                Console.WriteLine($"Invalid time format: {arg.Substring(7)}");
            }
        }
        else 
        {
            Console.WriteLine($"Unknown option: {arg}");
        }
        continue;
    }

    HandleFile(arg, noCreate, specifiedTime);
}

static void HandleFile(string filePath, bool noCreate, DateTime? specifiedTime)
{
    try 
    {
        if (!File.Exists(filePath))
        {
            if (noCreate)
            {
                Console.WriteLine($"File does not exist: {filePath} and --no-create is set.");
                return;
            }

            using (File.Create(filePath)) {}
        }

        DateTime timeToSet = specifiedTime ?? DateTime.Now;
        File.SetLastWriteTime(filePath, timeToSet);
        File.SetLastAccessTime(filePath, timeToSet);

        Console.WriteLine($"Updated timestamp for '{filePath}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling file '{filePath}': {ex.Message}");
    }
}