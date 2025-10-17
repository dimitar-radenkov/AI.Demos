using AI.Agents.Analysis;
using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace AI.Agents.CodeGeneration;

public sealed partial class DeveloperAgent : IAgent<CodeArtifactResult>
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public DeveloperAgent(IOptions<AgentSettings> options)
    {
        var settings = options.Value;
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

    public async Task<CodeArtifactResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to deserialize as Requirements first, then CodeReview (for feedback loop)
            Requirements? requirements = null;
            string? feedbackContext = null;

            try
            {
                requirements = System.Text.Json.JsonSerializer.Deserialize<Requirements>(input, JsonSerializerOptions.Default);
            }
            catch
            {
                // If not Requirements, try CodeReview (feedback from reviewer)
                try
                {
                    var codeReview = System.Text.Json.JsonSerializer.Deserialize<QualityAssurance.CodeReview>(input, JsonSerializerOptions.Default);
                    if (codeReview != null)
                    {
                        feedbackContext = $"Previous code was rejected. Review feedback:\n{codeReview.Comments}";
                        // Extract requirements from the feedback if available
                        requirements = new Requirements 
                        { 
                            Task = "Improve code based on feedback",
                            Inputs = Array.Empty<string>(),
                            Outputs = Array.Empty<string>(),
                            Constraints = Array.Empty<string>()
                        };
                    }
                }
                catch
                {
                    // If both fail, treat input as plain text requirements
                    requirements = new Requirements 
                    { 
                        Task = input,
                        Inputs = Array.Empty<string>(),
                        Outputs = Array.Empty<string>(),
                        Constraints = Array.Empty<string>()
                    };
                }
            }

            if (requirements == null)
            {
                return CodeArtifactResult.Failure("Failed to parse input as Requirements or CodeReview");
            }

            var prompt = BuildCodeGenerationPrompt(requirements, feedbackContext);
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
        catch (Exception ex)
        {
            return CodeArtifactResult.Failure($"Failed to generate code: {ex.Message}");
        }
    }

    private static string BuildCodeGenerationPrompt(Requirements requirements, string? feedbackContext = null)
    {
        var prompt = string.Empty;
        
        if (!string.IsNullOrEmpty(feedbackContext))
        {
            prompt += $"{feedbackContext}\n\n";
        }
        
        prompt += $"Generate C# code that implements the following requirements:\n\n" +
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