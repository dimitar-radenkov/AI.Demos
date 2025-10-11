using AI.Agents.CodeGeneration;
using AI.Agents.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the Developer Agent functionality.
/// Provides an interactive interface to test code generation from structured requirements.
/// </summary>
public partial class TestDeveloper : ComponentBase
{
    [Inject]
    private IDeveloperAgent DeveloperAgent { get; set; } = default!;

    [Inject]
    private ILogger<TestDeveloper> Logger { get; set; } = default!;

    /// <summary>
    /// The requirements to generate code for.
    /// </summary>
    protected Requirements Requirements { get; set; } = new()
    {
        Task = "",
        Inputs = [],
        Outputs = [],
        Constraints = []
    };

    /// <summary>
    /// Bound task text.
    /// </summary>
    protected string TaskText { get; set; } = "Create a function that adds two numbers together";

    /// <summary>
    /// Bound inputs text.
    /// </summary>
    protected string InputsText { get; set; } = "Two integer numbers";

    /// <summary>
    /// Bound outputs text.
    /// </summary>
    protected string OutputsText { get; set; } = "The sum of the two numbers";

    /// <summary>
    /// Bound constraints text.
    /// </summary>
    protected string ConstraintsText { get; set; } = "Use C# 13 features\nInclude input validation";

    /// <summary>
    /// Updates the Requirements object from the bound text properties.
    /// </summary>
    private void UpdateRequirements()
    {
        Requirements = new Requirements
        {
            Task = TaskText,
            Inputs = InputsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries),
            Outputs = OutputsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries),
            Constraints = ConstraintsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
        };
    }

    /// <summary>
    /// The result of the code generation.
    /// </summary>
    protected CodeArtifactResult? CodeResult { get; set; }

    /// <summary>
    /// Indicates whether code generation is currently in progress.
    /// </summary>
    protected bool IsGenerating { get; set; }

    /// <summary>
    /// Generates code using the Developer Agent.
    /// </summary>
    protected async Task GenerateCode()
    {
        UpdateRequirements();
        CodeResult = null;
        IsGenerating = true;

        try
        {
            CodeResult = await DeveloperAgent.GenerateCodeAsync(Requirements);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating code");
            CodeResult = CodeArtifactResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    /// <summary>
    /// Loads a simple arithmetic example.
    /// </summary>
    protected void LoadExampleSimple()
    {
        Requirements = new Requirements
        {
            Task = "Create a function that adds two numbers together",
            Inputs = ["Two integer numbers"],
            Outputs = ["The sum of the two numbers"],
            Constraints = ["Use C# 13 features", "Include input validation", "Return an integer result"]
        };
        CodeResult = null;
    }

    /// <summary>
    /// Loads a user greeting example.
    /// </summary>
    protected void LoadExampleGreeting()
    {
        Requirements = new Requirements
        {
            Task = "Write a program that greets a user by name",
            Inputs = ["User's name as a string"],
            Outputs = ["A personalized greeting message"],
            Constraints = ["Handle empty or null names gracefully", "Use string interpolation", "Keep it simple and readable"]
        };
        CodeResult = null;
    }

    /// <summary>
    /// Loads an email validation example.
    /// </summary>
    protected void LoadExampleValidation()
    {
        Requirements = new Requirements
        {
            Task = "Make a function that checks if an email address is valid",
            Inputs = ["Email address as a string"],
            Outputs = ["Boolean indicating if the email is valid"],
            Constraints = ["Use regular expressions", "Follow common email validation rules", "Return false for null/empty input"]
        };
        CodeResult = null;
    }

    /// <summary>
    /// Loads a todo list example.
    /// </summary>
    protected void LoadExampleTodo()
    {
        Requirements = new Requirements
        {
            Task = "Build a todo list that lets users add tasks and mark them as complete",
            Inputs = ["Task descriptions to add", "Task IDs to mark complete"],
            Outputs = ["List of current tasks with completion status"],
            Constraints = ["Use a collection to store tasks", "Each task should have an ID and description", "Support adding and completing tasks"]
        };
        CodeResult = null;
    }

    /// <summary>
    /// Loads a temperature converter example.
    /// </summary>
    protected void LoadExampleConverter()
    {
        Requirements = new Requirements
        {
            Task = "Create a temperature converter that changes Celsius to Fahrenheit",
            Inputs = ["Temperature in Celsius (double)"],
            Outputs = ["Temperature in Fahrenheit (double)"],
            Constraints = ["Use the formula: F = C × 9/5 + 32", "Handle decimal precision appropriately", "Include input validation"]
        };
        CodeResult = null;
    }

    /// <summary>
    /// Loads a word counter example.
    /// </summary>
    protected void LoadExampleCounter()
    {
        Requirements = new Requirements
        {
            Task = "Write a program that reads a text file and counts the words",
            Inputs = ["File path as a string"],
            Outputs = ["Word count as an integer"],
            Constraints = ["Handle file not found errors", "Split on whitespace and punctuation", "Return 0 for empty files"]
        };
        CodeResult = null;
    }
}