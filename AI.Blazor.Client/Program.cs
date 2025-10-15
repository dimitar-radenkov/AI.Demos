using AI.Agents;
using AI.Agents.CodeGeneration;
using AI.Agents.Analysis;
using AI.Agents.QualityAssurance;
using AI.Services.CodeExecution;
using AI.Services.Plugins.Agents;
using AI.Blazor.Client.Components;
using AI.Blazor.Client.Services.Chat;
using AI.Blazor.Client.Services.Markdown;
using AI.Blazor.Client.Services.Welcome;
using AI.Core.Settings;
using AI.Core.Settings.Agents;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure settings
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection(LlmSettings.SectionName));
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection(ChatSettings.SectionName));
builder.Services.Configure<WelcomeSettings>(builder.Configuration.GetSection(WelcomeSettings.SectionName));
builder.Services.Configure<FileIOSettings>(builder.Configuration.GetSection(FileIOSettings.SectionName));
builder.Services.Configure<AgentsSettings>(builder.Configuration.GetSection(AgentsSettings.SectionName));
builder.Services.Configure<CodeExecutionSettings>(builder.Configuration.GetSection(CodeExecutionSettings.SectionName));

// Register AI agents
builder.Services.AddScoped<IAgent<Requirements, CodeArtifactResult>, DeveloperAgent>();
builder.Services.AddScoped<IAgent<string, RequirementsResult>, QueryAnalystAgent>();
builder.Services.AddScoped<IAgent<CodeArtifact, CodeQualityResult>, QAAgent>();

// Register AI plugins and services
builder.Services.AddScoped<QAPlugin>();
builder.Services.AddScoped<ICodeExecutionService, CodeExecutionService>();

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
