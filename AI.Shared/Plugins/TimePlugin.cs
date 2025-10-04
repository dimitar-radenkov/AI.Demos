using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AI.Shared.Plugins;

public sealed class TimePlugin
{
    [KernelFunction]
    [Description("Gets the current local date and time.")]
    public static DateTime CurrentTime() => DateTime.Now;

    [KernelFunction]
    [Description("Gets the current UTC date and time.")]
    public static DateTime UtcTime() => DateTime.UtcNow;
}