using AI.Shared.Settings;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace AI.Blazor.Client.Agents.CodeGeneration;

public sealed partial class DotNetDeveloperAgent : IDotNetDeveloperAgent
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public DotNetDeveloperAgent(IOptions<AgentsSettings> agentsSettings)
    {
        var settings = agentsSettings.Value.DotNetDeveloper;

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(instructions: settings.GetSystemPrompt());

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<string> GenerateCodeAsync(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        var response = await this.agent.RunAsync(userInput, this.agentThread, cancellationToken: cancellationToken);

        return ExtractCodeFromResponse(response.Text);
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