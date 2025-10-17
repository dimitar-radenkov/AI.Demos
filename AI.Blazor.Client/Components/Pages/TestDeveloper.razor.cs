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
    private IAgent<CodeArtifactResult> DeveloperAgent { get; set; } = default!;

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
        this.Requirements = new Requirements
        {
            Task = this.TaskText,
            Inputs = this.InputsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries),
            Outputs = this.OutputsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries),
            Constraints = this.ConstraintsText.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
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
        this.UpdateRequirements();
        this.CodeResult = null;
        this.IsGenerating = true;

        try
        {
            var jsonInput = System.Text.Json.JsonSerializer.Serialize(this.Requirements);
            this.CodeResult = await this.DeveloperAgent.ExecuteAsync(jsonInput);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error generating code");
            this.CodeResult = CodeArtifactResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            this.IsGenerating = false;
        }
    }

    /// <summary>
    /// Loads a simple arithmetic example.
    /// </summary>
    protected void LoadExampleSimple()
    {
        this.TaskText = "Create a function that adds two numbers together";
        this.InputsText = "Two integer numbers";
        this.OutputsText = "The sum of the two numbers";
        this.ConstraintsText = "Use C# 13 features\nInclude input validation\nReturn an integer result";
        this.CodeResult = null;
    }

    /// <summary>
    /// Loads a user greeting example.
    /// </summary>
    protected void LoadExampleGreeting()
    {
        this.TaskText = "Write a program that greets a user by name";
        this.InputsText = "User's name as a string";
        this.OutputsText = "A personalized greeting message";
        this.ConstraintsText = "Handle empty or null names gracefully\nUse string interpolation\nKeep it simple and readable";
        this.CodeResult = null;
    }

    /// <summary>
    /// Loads an email validation example.
    /// </summary>
    protected void LoadExampleValidation()
    {
        this.TaskText = "Make a function that checks if an email address is valid";
        this.InputsText = "Email address as a string";
        this.OutputsText = "Boolean indicating if the email is valid";
        this.ConstraintsText = "Use regular expressions\nFollow common email validation rules\nReturn false for null/empty input";
        this.CodeResult = null;
    }

    /// <summary>
    /// Loads a todo list example.
    /// </summary>
    protected void LoadExampleTodo()
    {
        this.TaskText = "Build a todo list that lets users add tasks and mark them as complete";
        this.InputsText = "Task descriptions to add\nTask IDs to mark complete";
        this.OutputsText = "List of current tasks with completion status";
        this.ConstraintsText = "Use a collection to store tasks\nEach task should have an ID and description\nSupport adding and completing tasks";
        this.CodeResult = null;
    }

    /// <summary>
    /// Loads a temperature converter example.
    /// </summary>
    protected void LoadExampleConverter()
    {
        this.TaskText = "Create a temperature converter that changes Celsius to Fahrenheit";
        this.InputsText = "Temperature in Celsius (double)";
        this.OutputsText = "Temperature in Fahrenheit (double)";
        this.ConstraintsText = "Use the formula: F = C × 9/5 + 32\nHandle decimal precision appropriately\nInclude input validation";
        this.CodeResult = null;
    }

    /// <summary>
    /// Loads a word counter example.
    /// </summary>
    protected void LoadExampleCounter()
    {
        this.TaskText = "Write a program that reads a text file and counts the words";
        this.InputsText = "File path as a string";
        this.OutputsText = "Word count as an integer";
        this.ConstraintsText = "Handle file not found errors\nSplit on whitespace and punctuation\nReturn 0 for empty files";
        this.CodeResult = null;
    }
}