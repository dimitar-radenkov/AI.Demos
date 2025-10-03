namespace AI.Client.Utils;

public static class ConsoleHelper
{
    public static void WriteUser(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("User: ");
        Console.WriteLine(message);
        Console.WriteLine();
        Console.ResetColor();
    }

    public static string ReadUserInput()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("User: ");
        Console.ResetColor();
        return Console.ReadLine() ?? string.Empty;
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {message}");
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void WriteAssistant(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Assistant: ");
        Console.WriteLine(message);
        Console.WriteLine();
        Console.ResetColor();
    }
}