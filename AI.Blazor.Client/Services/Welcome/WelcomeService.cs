using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace AI.Blazor.Client.Services.Welcome;

public sealed class WelcomeService : IWelcomeService
{
    private static readonly KernelFunction WelcomeFunction = KernelFunctionFactory.CreateFromPrompt(
        promptTemplate: """
            Generate a personalized, friendly welcome message.

            User's name: {{$userName}}
            Current date and time: {{$currentDateTime}}

            Requirements:
            - Greet the user by name
            - Reference the current time contextually (morning/afternoon/evening)
            - Keep it warm and concise (2 sentences)
            - End with an offer to help

            Output only the welcome message.
            """,
        functionName: "GenerateWelcome",
        description: "Generates personalized welcome messages");

    private readonly Kernel kernel;
    private readonly WelcomeSettings settings;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<WelcomeService> logger;

    public WelcomeService(
        Kernel kernel,
        IOptions<WelcomeSettings> options,
        TimeProvider timeProvider,
        ILogger<WelcomeService> logger)
    {
        this.kernel = kernel;
        this.settings = options.Value;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<string> GenerateWelcomeMessage(
        CancellationToken cancellationToken = default)
    {
        var arguments = new KernelArguments
        {
            ["userName"] = this.settings.UserName,
            ["currentDateTime"] = this.timeProvider.GetLocalNow().ToString("F") // Full date/time format
        };

        try
        {
            var result = await this.kernel.InvokeAsync(
                WelcomeFunction,
                arguments,
                cancellationToken);

            return result.GetValue<string>()
                ?? GenerateFallbackMessage(settings.UserName);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error generating welcome message");
            return GenerateFallbackMessage(settings.UserName);
        }
    }

    private static string GenerateFallbackMessage(string userName) =>
        $"Hello {userName}! How can I help you today?";
}