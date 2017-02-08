using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

class Program
{
    static string Authority = "";
    static object logLock = new object();

    static void Main(string[] args)
    {
        Log("ADFS via ADAL Test", ConsoleColor.Cyan);
        Log();
        Log("Connecting to ADFS authority");
        var cxt = new AuthenticationContext(Authority, false);
        var token = cxt.AcquireTokenAsync("test", new ClientCredential("", "")).Result;
    }

    static void Log(string message = "", ConsoleColor? color = null)
    {
        lock (logLock)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            if (color.HasValue) Console.ResetColor();
        }
    }
}