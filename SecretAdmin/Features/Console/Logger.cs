using System;
using System.IO;
using SecretAdmin.Features.Server;

namespace SecretAdmin.Features.Console
{
    public class Logger
    {
        private readonly string _path;

        public Logger(string path)
        {
            _path = path;
        }

        public void AppendLog(string message, bool newLine = false)
        {
            using (var stream = File.AppendText(_path))
            {
                if (newLine)
                    stream.WriteLine(message);
                else
                    stream.Write(message);
            }
        }
    }
}