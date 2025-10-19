using AI.Core.Settings;

namespace AI.Client.Settings;

public static class ScriptRunnerSettingsProvider
{
    public static ScriptRunnerSettings Create()
    {
        return new ScriptRunnerSettings
        {
            MaxExecutionTimeSeconds = 5,
            MaxConcurrentExecutions = 2,
            AllowedNamespaces =
            [
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text"
            ],
            AllowedAssemblies =
            [
                "System.Runtime",
                "System.Linq",
                "System.Collections"
            ]
        };
    }
}
