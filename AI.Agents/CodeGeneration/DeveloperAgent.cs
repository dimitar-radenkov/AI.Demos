using AI.Agents.Analysis;
using AI.Shared.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace AI.Agents.CodeGeneration;

public sealed partial class DeveloperAgent : IDeveloperAgent
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public DeveloperAgent(IOptions<AgentsSettings> agentsSettings)
    {
        var settings = agentsSettings.Value.Developer;

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                name: "Developer-Agent",
                instructions: settings.GetSystemPrompt());

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<CodeArtifactResult> GenerateCodeAsync(
        Requirements requirements,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildCodeGenerationPrompt(requirements);
        var response = await this.agent.RunAsync(prompt, this.agentThread, cancellationToken: cancellationToken);

        var code = ExtractCodeFromResponse(response.Text);

        var codeArtifact = new CodeArtifact
        {
            Code = code,
            Language = "csharp",
            GeneratedAt = DateTime.UtcNow,
            Requirements = requirements
        };

        return CodeArtifactResult.Success(codeArtifact);
    }

    private static string BuildCodeGenerationPrompt(Requirements requirements)
    {
        var prompt = $"Generate C# code that implements the following requirements:\n\n" +
                     $"Task: {requirements.Task}\n";

        if (requirements.Inputs.Length > 0)
        {
            prompt += $"Inputs: {string.Join(", ", requirements.Inputs)}\n";
        }

        if (requirements.Outputs.Length > 0)
        {
            prompt += $"Outputs: {string.Join(", ", requirements.Outputs)}\n";
        }

        if (requirements.Constraints.Length > 0)
        {
            prompt += $"Constraints: {string.Join(", ", requirements.Constraints)}\n";
        }

        prompt += "\nProvide only the C# code implementation, properly formatted and with necessary using statements.";

        return prompt;
    }

    private static string ExtractCodeFromResponse(string response)
    {
        // Look for ```csharp code blocks first
        var csharpMatch = DotNetOutputRegex().Match(response);
        if (csharpMatch.Success)
        {
            return csharpMatch.Groups[1].Value.Trim();
        }

        // Fallback: look for any code block
        var codeMatch = FallBackRegex().Match(response);
        if (codeMatch.Success)
        {
            return codeMatch.Groups[1].Value.Trim();
        }

        // If no code blocks found, return the full response
        // This handles cases where the AI doesn't use markdown formatting
        return response.Trim();
    }

    [GeneratedRegex(@"```csharp\s*\n(.*?)\n```", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex DotNetOutputRegex();

    [GeneratedRegex(@"```\s*\n(.*?)\n```", RegexOptions.Singleline)]
    private static partial Regex FallBackRegex();
}