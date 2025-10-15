using AI.Agents;
using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using Microsoft.AspNetCore.Components;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the Developer Agent functionality.
/// Provides an interactive interface to test code generation from structured requirements.
/// </summary>
public partial class TestDeveloper : ComponentBase
{
    [Inject]
    private IAgent<Requirements, CodeArtifactResult> DeveloperAgent { get; set; } = default!;

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
            CodeResult = await DeveloperAgent.ExecuteAsync(Requirements);
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
        TaskText = "Create a function that adds two numbers together";
        InputsText = "Two integer numbers";
        OutputsText = "The sum of the two numbers";
        ConstraintsText = "Use C# 13 features\nInclude input validation\nReturn an integer result";
        CodeResult = null;
    }

    /// <summary>
    /// Loads a user greeting example.
    /// </summary>
    protected void LoadExampleGreeting()
    {
        TaskText = "Write a program that greets a user by name";
        InputsText = "User's name as a string";
        OutputsText = "A personalized greeting message";
        ConstraintsText = "Handle empty or null names gracefully\nUse string interpolation\nKeep it simple and readable";
        CodeResult = null;
    }

    /// <summary>
    /// Loads an email validation example.
    /// </summary>
    protected void LoadExampleValidation()
    {
        TaskText = "Make a function that checks if an email address is valid";
        InputsText = "Email address as a string";
        OutputsText = "Boolean indicating if the email is valid";
        ConstraintsText = "Use regular expressions\nFollow common email validation rules\nReturn false for null/empty input";
        CodeResult = null;
    }

    /// <summary>
    /// Loads a todo list example.
    /// </summary>
    protected void LoadExampleTodo()
    {
        TaskText = "Build a todo list that lets users add tasks and mark them as complete";
        InputsText = "Task descriptions to add\nTask IDs to mark complete";
        OutputsText = "List of current tasks with completion status";
        ConstraintsText = "Use a collection to store tasks\nEach task should have an ID and description\nSupport adding and completing tasks";
        CodeResult = null;
    }

    /// <summary>
    /// Loads a temperature converter example.
    /// </summary>
    protected void LoadExampleConverter()
    {
        TaskText = "Create a temperature converter that changes Celsius to Fahrenheit";
        InputsText = "Temperature in Celsius (double)";
        OutputsText = "Temperature in Fahrenheit (double)";
        ConstraintsText = "Use the formula: F = C × 9/5 + 32\nHandle decimal precision appropriately\nInclude input validation";
        CodeResult = null;
    }

    /// <summary>
    /// Loads a word counter example.
    /// </summary>
    protected void LoadExampleCounter()
    {
        TaskText = "Write a program that reads a text file and counts the words";
        InputsText = "File path as a string";
        OutputsText = "Word count as an integer";
        ConstraintsText = "Handle file not found errors\nSplit on whitespace and punctuation\nReturn 0 for empty files";
        CodeResult = null;
    }
}