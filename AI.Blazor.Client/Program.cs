using AI.Agents;
using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Presentation;
using AI.Agents.QualityAssurance;
using AI.Blazor.Client.Components;
using AI.Blazor.Client.Services.Chat;
using AI.Blazor.Client.Services.Markdown;
using AI.Blazor.Client.Services.Welcome;
using AI.Core.Settings;
using AI.Core.Settings.Agents;
using AI.Services.CodeExecution;
using AI.Services.Plugins.Agents;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure settings
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection(LlmSettings.SectionName));
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection(ChatSettings.SectionName));
builder.Services.Configure<WelcomeSettings>(builder.Configuration.GetSection(WelcomeSettings.SectionName));
builder.Services.Configure<FileIOSettings>(builder.Configuration.GetSection(FileIOSettings.SectionName));
builder.Services.Configure<ScriptRunnerSettings>(builder.Configuration.GetSection(ScriptRunnerSettings.SectionName));

// Register AI agents with their specific configurations
builder.Services.AddScoped<IAgent<CodeArtifactResult>>(sp => 
{
    var options = Options.Create(builder.Configuration.GetSection(AgentConfigurationSections.Developer).Get<AgentSettings>()!);
    return new DeveloperAgent(options);
});

builder.Services.AddScoped<IAgent<RequirementsResult>>(sp => 
{
    var options = Options.Create(builder.Configuration.GetSection(AgentConfigurationSections.QueryAnalyst).Get<AgentSettings>()!);
    return new QueryAnalystAgent(options);
});

builder.Services.AddScoped<IAgent<CodeQualityResult>>(sp =>
{
    var qaPlugin = sp.GetRequiredService<QAPlugin>();
    var logger = sp.GetRequiredService<ILogger<QAAgent>>();
    var options = Options.Create(builder.Configuration.GetSection(AgentConfigurationSections.QA).Get<AgentSettings>()!);
    return new QAAgent(options, qaPlugin, logger);
});

builder.Services.AddScoped<IAgent<PresentationResult>>(sp =>
{
    var options = Options.Create(builder.Configuration.GetSection(AgentConfigurationSections.Presenter).Get<AgentSettings>()!);
    return new PresenterAgent(options);
});

// Register AI plugins and services
builder.Services.AddScoped<QAPlugin>();
builder.Services.AddScoped<IScriptRunner, RoslynScriptRunner>();

// Creates TRANSIENT kernel instance for each request
var llmOptions = builder.Configuration.GetSection(LlmSettings.SectionName).Get<LlmSettings>();
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: llmOptions!.Model,
    apiKey: llmOptions.ApiKey,
    endpoint: new Uri($"{llmOptions.BaseUrl}/v1"));

kernelBuilder.Plugins.AddFromType<AI.Services.Plugins.TimePlugin>();
kernelBuilder.Plugins.AddFromType<AI.Services.Plugins.FileManagementPlugin>();
kernelBuilder.Plugins.AddFromType<AI.Services.Plugins.CalculatorPlugin>();

// Register application services
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IWelcomeService, WelcomeService>();
builder.Services.AddSingleton<IMarkdownService, MarkdigService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
