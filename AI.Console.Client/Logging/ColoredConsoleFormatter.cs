using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace AI.Console.Client.Logging;

public sealed class ColoredConsoleFormatter : ConsoleFormatter
{
    private static readonly Dictionary<string, string> CategoryColors = new()
    {
        ["Program"] = "\x1b[96m",
        ["AI.Agents.Pipeline.Executors.AnalystExecutor"] = "\x1b[92m",
        ["AI.Agents.Pipeline.Executors.DeveloperExecutor"] = "\x1b[93m",
        ["AI.Agents.Pipeline.Executors.ReviewerExecutor"] = "\x1b[95m",
        ["AI.Agents.Pipeline.Executors.ScriptExecutionExecutor"] = "\x1b[91m",
        ["AI.Agents.Pipeline.Executors.PresenterExecutor"] = "\x1b[94m"
    };

    private const string ResetColor = "\x1b[0m";
    private const string DefaultColor = "\x1b[37m";

    private readonly ColoredConsoleFormatterOptions options;

    public ColoredConsoleFormatter(IOptionsMonitor<ColoredConsoleFormatterOptions> options)
        : base("colored")
    {
        this.options = options.CurrentValue;
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception == null)
        {
            return;
        }

        var logLevel = logEntry.LogLevel;
        var category = logEntry.Category;

        var colorCode = options.EnableColors ? GetColorForCategory(category) : string.Empty;
        var resetCode = options.EnableColors ? ResetColor : string.Empty;

        var logLevelString = GetLogLevelString(logLevel);
        var timestamp = DateTime.Now.ToString("HH:mm:ss");

        textWriter.Write($"{colorCode}[{timestamp}] ");
        textWriter.Write($"{logLevelString}: ");
        textWriter.Write($"{GetSimplifiedCategoryName(category)} - ");
        textWriter.Write(message);

        if (logEntry.Exception != null)
        {
            textWriter.Write($" | Exception: {logEntry.Exception.Message}");
        }

        textWriter.WriteLine(resetCode);
    }

    private static string GetColorForCategory(string category)
    {
        if (CategoryColors.TryGetValue(category, out var color))
        {
            return color;
        }

        foreach (var kvp in CategoryColors)
        {
            if (category.Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }

        return DefaultColor;
    }

    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => "unkn"
    };

    private static string GetSimplifiedCategoryName(string category)
    {
        if (category.Contains("AnalystExecutor"))
            return "Analyst";
        if (category.Contains("DeveloperExecutor"))
            return "Developer";
        if (category.Contains("ReviewerExecutor"))
            return "Reviewer";
        if (category.Contains("ScriptExecutionExecutor"))
            return "Execution";
        if (category.Contains("PresenterExecutor"))
            return "Presenter";
        if (category == "Program")
            return "Workflow";

        return category.Split('.').LastOrDefault() ?? category;
    }
}
