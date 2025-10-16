namespace AI.Console.Client;

public static class ConsoleHelper
{
    public static void WriteUser(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.Write("User: ");
        System.Console.WriteLine(message);
        System.Console.WriteLine();
        System.Console.ResetColor();
    }

    public static string ReadUserInput()
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.Write("User: ");
        System.Console.ResetColor();
        return System.Console.ReadLine() ?? string.Empty;
    }

    public static void WriteWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"Warning: {message}");
        System.Console.WriteLine();
        System.Console.ResetColor();
    }

    public static void WriteAssistant(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.Write("Assistant: ");
        System.Console.WriteLine(message);
        System.Console.WriteLine();
        System.Console.ResetColor();
    }
}