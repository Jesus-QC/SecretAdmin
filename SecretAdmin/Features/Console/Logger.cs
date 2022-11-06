using System.IO;

namespace SecretAdmin.Features.Console;

public class Logger
{
    private readonly string _path;

    public Logger(string path)
    {
        _path = path;
    }

    public void AppendLog(object message, bool newLine = true)
    {
        using StreamWriter stream = File.AppendText(_path);
            
        if (newLine)
            stream.WriteLine(message);
        else
            stream.Write(message);
    }
}