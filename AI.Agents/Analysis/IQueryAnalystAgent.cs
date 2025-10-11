using AI.Agents.Models;

namespace AI.Agents.Analysis;

/// <summary>
/// Defines the contract for an agent that analyzes user requests and extracts structured requirements.
/// This agent is the first in the code generation workflow, responsible for understanding
/// what the user wants and converting natural language into structured requirements.
/// </summary>
public interface IQueryAnalystAgent
{
    /// <summary>
    /// Analyzes a user request and extracts structured requirements for code generation.
    /// </summary>
    /// <param name="userRequest">The natural language request from the user describing what code they want.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A <see cref="RequirementsResult"/> indicating success with the structured requirements or failure with an error message.</returns>
    Task<RequirementsResult> AnalyzeRequestAsync(string userRequest, CancellationToken cancellationToken = default);
}