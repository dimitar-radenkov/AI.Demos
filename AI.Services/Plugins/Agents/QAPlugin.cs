using System.ComponentModel;
using AI.Services.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AI.Services.Plugins.Agents;

public sealed class QAPlugin
{
    private readonly ICodeExecutionService codeExecutionService;
    private readonly ILogger<QAPlugin> logger;

    public QAPlugin(ICodeExecutionService codeExecutionService, ILogger<QAPlugin> logger)
    {
        this.codeExecutionService = codeExecutionService;
        this.logger = logger;
    }

    [Description("Validates C# code for compilation errors. Returns 'success' if code compiles, or error message if compilation fails.")]
    public async Task<string> ValidateCode(
        [Description("The C# code to validate for compilation")] string code)
    {
        this.logger.LogInformation("Tool: ValidateCode called with {Length} characters", code?.Length ?? 0);

        var result = await this.codeExecutionService.ValidateCode(code);

        if (result.IsSuccess)
        {
            return "success";
        }

        return $"Compilation failed: {result.ErrorMessage}";
    }

    [Description("Executes validated C# code and returns the result. Only use this after code has been validated and approved. Returns the execution result or error message.")]
    public async Task<string> ExecuteCode(
        [Description("The C# code to execute")] string code)
    {
        this.logger.LogInformation("Tool: ExecuteCode called with {Length} characters", code?.Length ?? 0);

        var result = await this.codeExecutionService.ExecuteCode(code, CancellationToken.None);

        if (result.IsSuccess && result.Data != null)
        {
            var output = result.Data.ReturnValue?.ToString() ?? "(no output)";
            var time = result.Data.ExecutionTime.TotalMilliseconds;
            return $"Execution successful in {time:F2}ms. Output: {output}";
        }

        var errorMessage = result.ErrorMessage ?? "Execution failed";
        return $"Execution failed: {errorMessage}";
    }
}
