using AI.Services.CodeExecution;
using AI.Services.CodeExecution.Models;
using Microsoft.AspNetCore.Components;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the Code Execution Agent functionality.
/// Provides an interactive interface to test dynamic C# code execution.
/// </summary>
public partial class CodeExecutionTest : ComponentBase
{
    [Inject]
    private IScriptRunner ScriptRunner { get; set; } = default!;

    [Inject]
    private ILogger<CodeExecutionTest> Logger { get; set; } = default!;

    /// <summary>
    /// The C# code to execute.
    /// </summary>
    protected string Code { get; set; } = "1 + 2";

    /// <summary>
    /// The result of code execution.
    /// </summary>
    protected ExecutionResult? ExecutionResult { get; set; }

    /// <summary>
    /// The result of code validation.
    /// </summary>
    protected ValidationResult? ValidationResult { get; set; }

    /// <summary>
    /// Indicates whether code is currently being executed or validated.
    /// </summary>
    protected bool IsExecuting { get; set; }

    /// <summary>
    /// Executes the C# code using the script runner.
    /// </summary>
    protected async Task ExecuteCode()
    {
        this.ExecutionResult = null;
        this.ValidationResult = null;
        this.IsExecuting = true;

        try
        {
            this.ExecutionResult = await this.ScriptRunner.ExecuteAsync(this.Code);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error executing code");
            this.ExecutionResult = ExecutionResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            this.IsExecuting = false;
        }
    }

    /// <summary>
    /// Validates the C# code without executing it.
    /// </summary>
    protected async Task ValidateCode()
    {
        this.ExecutionResult = null;
        this.ValidationResult = null;
        this.IsExecuting = true;

        try
        {
            this.ValidationResult = await this.ScriptRunner.ValidateAsync(this.Code);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error validating code");
            this.ValidationResult = ValidationResult.Failure($"Validation error: {ex.Message}");
        }
        finally
        {
            this.IsExecuting = false;
        }
    }

    /// <summary>
    /// Loads an example code snippet into the code editor.
    /// </summary>
    /// <param name="exampleCode">The example code to load.</param>
    protected void LoadExample(string exampleCode)
    {
        this.Code = exampleCode;
        this.ExecutionResult = null;
        this.ValidationResult = null;
    }
}