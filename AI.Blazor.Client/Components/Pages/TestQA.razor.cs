using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the QA Agent functionality.
/// Provides an interactive interface to test code validation and execution.
/// </summary>
public partial class TestQA : ComponentBase
{
    [Inject]
    private IQAAgent QAAgent { get; set; } = default!;

    [Inject]
    private ILogger<TestQA> Logger { get; set; } = default!;

    /// <summary>
    /// The code input from the user.
    /// </summary>
    protected string CodeInput { get; set; } = "return 1 + 1;";

    /// <summary>
    /// The result of the QA test.
    /// </summary>
    protected CodeQualityResult? TestResult { get; set; }

    /// <summary>
    /// Indicates whether testing is currently in progress.
    /// </summary>
    protected bool IsTesting { get; set; }

    /// <summary>
    /// Validates and tests the code using the QA Agent.
    /// </summary>
    protected async Task Validate()
    {
        TestResult = null;
        IsTesting = true;

        try
        {
            var artifact = new CodeArtifact
            {
                Code = CodeInput,
                Language = "csharp",
                GeneratedAt = DateTime.UtcNow,
                Requirements = new Requirements
                {
                    Task = "Test execution",
                    Inputs = Array.Empty<string>(),
                    Outputs = Array.Empty<string>(),
                    Constraints = Array.Empty<string>()
                }
            };

            TestResult = await QAAgent.Validate(artifact);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error testing code");
            TestResult = CodeQualityResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            IsTesting = false;
        }
    }

    /// <summary>
    /// Loads an example code snippet.
    /// </summary>
    protected void LoadExample(string code)
    {
        CodeInput = code;
        TestResult = null;
    }

    /// <summary>
    /// Gets the CSS class for the overall result container.
    /// </summary>
    protected string GetResultClass()
    {
        if (TestResult?.IsSuccess == true && TestResult.Data?.IsSuccessful == true)
        {
            return "result-success";
        }

        return "result-error";
    }

    /// <summary>
    /// Gets the CSS class for validation status badge.
    /// </summary>
    protected string GetValidationStatusClass(CodeValidationStatus status)
    {
        return status switch
        {
            CodeValidationStatus.Passed => "bg-success",
            CodeValidationStatus.Failed => "bg-danger",
            CodeValidationStatus.Skipped => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Gets the CSS class for execution status badge.
    /// </summary>
    protected string GetExecutionStatusClass(CodeExecutionStatus status)
    {
        return status switch
        {
            CodeExecutionStatus.Success => "bg-success",
            CodeExecutionStatus.Failed => "bg-danger",
            CodeExecutionStatus.Rejected => "bg-warning",
            CodeExecutionStatus.NotExecuted => "bg-secondary",
            CodeExecutionStatus.Timeout => "bg-warning",
            _ => "bg-secondary"
        };
    }

    #region Example Code Snippets

    protected const string ExampleValid = @"return 1 + 1;";

    protected const string ExampleSyntaxError = @"int x = 5
return x * 2;";

    protected const string ExampleRuntimeError = @"int[] numbers = new int[5];
return numbers[10];";

    protected const string ExampleLoop = @"int sum = 0;
for (int i = 1; i <= 10; i++)
{
    sum += i;
}
return sum;";

    protected const string ExampleDateTime = @"var now = DateTime.Now;
var formatted = now.ToString(""yyyy-MM-dd HH:mm:ss"");
return formatted;";

    protected const string ExampleMath = @"double radius = 5.0;
double area = Math.PI * radius * radius;
return Math.Round(area, 2);";

    #endregion
}