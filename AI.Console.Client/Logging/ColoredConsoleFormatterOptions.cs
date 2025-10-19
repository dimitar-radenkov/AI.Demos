using Microsoft.Extensions.Logging.Console;

namespace AI.Console.Client.Logging;

public sealed class ColoredConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool EnableColors { get; set; } = true;
}
