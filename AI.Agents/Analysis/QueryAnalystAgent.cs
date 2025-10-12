using AI.Shared.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AI.Agents.Analysis;

public sealed partial class QueryAnalystAgent : IQueryAnalystAgent
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public QueryAnalystAgent(IOptions<AgentsSettings> agentsSettings)
    {
        var settings = agentsSettings.Value.QueryAnalyst;

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                name: "Query-Analyst-Agent",
                instructions: settings.GetSystemPrompt());

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<RequirementsResult> AnalyzeRequest(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.agent.RunAsync(userRequest, this.agentThread, cancellationToken: cancellationToken);

            var requirements = ParseRequirementsFromResponse(response.Text);
            return RequirementsResult.Success(requirements);
        }
        catch (Exception ex)
        {
            return RequirementsResult.Failure($"Failed to analyze request: {ex.Message}");
        }
    }

    private static Requirements ParseRequirementsFromResponse(string response)
    {
        try
        {
            // Try to parse as JSON first
            var requirements = JsonSerializer.Deserialize<Requirements>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (requirements != null)
            {
                return requirements;
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, try to extract from markdown or plain text
        }

        // Fallback: create basic requirements from the response text
        return new Requirements
        {
            Task = ExtractTaskFromText(response),
            Inputs = ExtractInputsFromText(response),
            Outputs = ExtractOutputsFromText(response),
            Constraints = ExtractConstraintsFromText(response)
        };
    }

    private static string ExtractTaskFromText(string text)
    {
        // Simple extraction logic - look for task-related keywords
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("task", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("create", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("write", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("implement", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }

        // Fallback: return first non-empty line
        return lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? "Unknown task";
    }

    private static string[] ExtractInputsFromText(string text)
    {
        // Look for input-related patterns
        var inputs = new List<string>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("input", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("parameter", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(trimmed);
            }
        }

        return inputs.Count > 0 ? inputs.ToArray() : new[] { "No specific inputs identified" };
    }

    private static string[] ExtractOutputsFromText(string text)
    {
        // Look for output-related patterns
        var outputs = new List<string>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("output", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("return", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("result", StringComparison.OrdinalIgnoreCase))
            {
                outputs.Add(trimmed);
            }
        }

        return outputs.Count > 0 ? outputs.ToArray() : new[] { "No specific outputs identified" };
    }

    private static string[] ExtractConstraintsFromText(string text)
    {
        // Look for constraint-related patterns
        var constraints = new List<string>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("requirement", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("must", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("should", StringComparison.OrdinalIgnoreCase))
            {
                constraints.Add(trimmed);
            }
        }

        return constraints.Count > 0 ? constraints.ToArray() : new[] { "No specific constraints identified" };
    }
}