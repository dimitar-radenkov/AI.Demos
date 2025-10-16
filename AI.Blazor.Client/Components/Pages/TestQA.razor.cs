using AI.Agents;
using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using Microsoft.AspNetCore.Components;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the QA Agent functionality.
/// Provides an interactive interface to test code validation and execution.
/// </summary>
public partial class TestQA : ComponentBase
{
    [Inject]
    private IAgent<CodeArtifact, CodeQualityResult> QAAgent { get; set; } = default!;

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
        this.TestResult = null;
        this.IsTesting = true;

        try
        {
            var artifact = new CodeArtifact
            {
                Code = this.CodeInput,
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

            this.TestResult = await this.QAAgent.ExecuteAsync(artifact);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error testing code");
            this.TestResult = CodeQualityResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            this.IsTesting = false;
        }
    }

    /// <summary>
    /// Loads an example code snippet.
    /// </summary>
    protected void LoadExample(string code)
    {
        this.CodeInput = code;
        this.TestResult = null;
    }

    /// <summary>
    /// Gets the CSS class for the overall result container.
    /// </summary>
    protected string GetResultClass()
    {
        if (this.TestResult?.IsSuccess == true)
        {
            return "result-success";
        }

        return "result-error";
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