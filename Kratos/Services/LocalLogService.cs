using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace Kratos.Services
{
    public class LocalLogService
    {
        private StreamWriter _logFile;

        public bool LogToFile { get; set; }

        public async Task Log(LogMessage m)
        {
            switch (m.Severity)
            {
                case LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogSeverity.Critical: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                case LogSeverity.Verbose: Console.ForegroundColor = ConsoleColor.White; break;
            }

            Console.WriteLine(m.ToString());
            if (m.Exception != null)
                Console.WriteLine(m.Exception);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (!LogToFile) return;
            if (_logFile == null) InitializeFileLogging();

            await _logFile.WriteLineAsync(m.ToString());
            await _logFile.FlushAsync();
        }

        private void InitializeFileLogging()
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "log"));
            string path = Path.Combine(Directory.GetCurrentDirectory(), "log", DateTime.UtcNow.ToString("dd-MM-yyyy--HH-mm-ss") + ".txt");
            _logFile = new StreamWriter(new FileStream(path, FileMode.CreateNew));
        }

        public LocalLogService()
        {
            LogToFile = false; // Placeholder until I can get proper CLI argument handling
        }
    }
}
