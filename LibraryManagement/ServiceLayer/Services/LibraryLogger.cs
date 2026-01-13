using System;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services
{
    /// <summary>
    /// Implementarea serviciului de logging.
    /// </summary>
    public class LibraryLogger : ILoggerService
    {
        public void LogInformation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO][{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN][{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        public void LogError(string message, Exception exception = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR][{DateTime.Now:HH:mm:ss}] {message}");
            if (exception != null) Console.WriteLine(exception.StackTrace);
            Console.ResetColor();
        }
    }
}