using AI.Agents;
using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Executors;
using AI.Agents.QualityAssurance;
using AI.Agents.Presentation;
using AI.Client.Settings;
using AI.Console.Client.Factories;
using AI.Console.Client.Settings;
using AI.Services.CodeExecution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Console.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddScoped<IAgent<RequirementsResult>>(sp =>
            new QueryAnalystAgent(
                Options.Create(AgentSettingsProvider.CreateQueryAnalystSettings())));

        services.AddScoped<IAgent<CodeArtifactResult>>(sp =>
            new DeveloperAgent(
                Options.Create(AgentSettingsProvider.CreateDeveloperSettings())));

        services.AddScoped<IAgent<CodeReviewResult>>(sp =>
            new ReviewerAgent(
                Options.Create(AgentSettingsProvider.CreateReviewerSettings())));

        services.AddScoped<IAgent<PresentationResult>>(sp =>
            new PresenterAgent(
                Options.Create(AgentSettingsProvider.CreatePresenterSettings())));

        return services;
    }

    public static IServiceCollection AddWorkflow(this IServiceCollection services)
    {
        services.AddScoped<AnalystExecutor>();
        services.AddScoped<DeveloperExecutor>();
        services.AddScoped<ReviewerExecutor>();
        services.AddScoped<ScriptExecutionExecutor>();
        services.AddScoped<PresenterExecutor>();
        services.AddScoped<IWorkflowFactory, WorkflowFactory>();

        return services;
    }

    public static IServiceCollection AddScriptRunner(this IServiceCollection services)
    {
        services.AddScoped<IScriptRunner>(sp =>
            new RoslynScriptRunner(
                Options.Create(ScriptRunnerSettingsProvider.Create()),
                sp.GetRequiredService<ILogger<RoslynScriptRunner>>()));

        return services;
    }
}
