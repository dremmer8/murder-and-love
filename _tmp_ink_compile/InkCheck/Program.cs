using System;
using System.IO;
using Ink;

class Program
{
    static int Main(string[] args)
    {
        string path = args[0];
        string outPath = args[1];
        string src = File.ReadAllText(path);
        int errors = 0;
        var compiler = new Compiler(src, new Compiler.Options
        {
            sourceFilename = Path.GetFileName(path),
            errorHandler = (msg, type) =>
            {
                Console.WriteLine(type + ": " + msg);
                if (type == ErrorType.Error)
                    errors++;
            }
        });
        var story = compiler.Compile();
        if (story == null)
        {
            Console.WriteLine("COMPILE FAILED (null story)");
            return 2;
        }
        string json = story.ToJson();
        File.WriteAllText(outPath, json);
        Console.WriteLine("OK errors=" + errors + " jsonLen=" + json.Length);
        return errors > 0 ? 1 : 0;
    }
}
