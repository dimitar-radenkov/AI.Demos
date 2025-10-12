using AI.Agents.Analysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the Query Analyst Agent functionality.
/// Provides an interactive interface to test natural language requirements analysis.
/// </summary>
public partial class TestQueryAnalyst : ComponentBase
{
    [Inject]
    private IQueryAnalystAgent QueryAnalystAgent { get; set; } = default!;

    [Inject]
    private ILogger<TestQueryAnalyst> Logger { get; set; } = default!;

    /// <summary>
    /// The user request to analyze.
    /// </summary>
    protected string UserRequest { get; set; } = "Create a function that adds two numbers together and returns the result.";

    /// <summary>
    /// The result of the requirements analysis.
    /// </summary>
    protected RequirementsResult? AnalysisResult { get; set; }

    /// <summary>
    /// Indicates whether analysis is currently in progress.
    /// </summary>
    protected bool IsAnalyzing { get; set; }

    /// <summary>
    /// Analyzes the user request using the Query Analyst Agent.
    /// </summary>
    protected async Task AnalyzeRequest()
    {
        AnalysisResult = null;
        IsAnalyzing = true;

        try
        {
            AnalysisResult = await QueryAnalystAgent.AnalyzeRequest(UserRequest);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error analyzing request");
            AnalysisResult = RequirementsResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    /// <summary>
    /// Loads an example request into the input field.
    /// </summary>
    /// <param name="exampleRequest">The example request to load.</param>
    protected void LoadExample(string exampleRequest)
    {
        UserRequest = exampleRequest;
        AnalysisResult = null;
    }
}