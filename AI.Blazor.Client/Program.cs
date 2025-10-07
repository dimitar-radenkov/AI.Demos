using AI.Blazor.Client.Agents.CodeGeneration;
using AI.Blazor.Client.Components;
using AI.Blazor.Client.Services.Chat;
using AI.Blazor.Client.Services.Markdown;
using AI.Blazor.Client.Services.Welcome;
using AI.Shared.Plugins;
using AI.Shared.Settings;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure settings
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection(LlmSettings.SectionName));
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection(ChatSettings.SectionName));
builder.Services.Configure<WelcomeSettings>(builder.Configuration.GetSection(WelcomeSettings.SectionName));
builder.Services.Configure<FileIOSettings>(builder.Configuration.GetSection(FileIOSettings.SectionName));
builder.Services.Configure<AgentsSettings>(builder.Configuration.GetSection(AgentsSettings.SectionName));

// Register the .NET code generation agent
builder.Services.AddScoped<IDotNetDeveloperAgent, DotNetDeveloperAgent>();

// Creates TRANSIENT kernel instance for each request
var llmOptions = builder.Configuration.GetSection(LlmSettings.SectionName).Get<LlmSettings>();
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: llmOptions!.Model,
    apiKey: llmOptions.ApiKey,
    endpoint: new Uri($"{llmOptions.BaseUrl}/v1"));

kernelBuilder.Plugins.AddFromType<AI.Shared.Plugins.TimePlugin>();
kernelBuilder.Plugins.AddFromType<FileManagementPlugin>();
kernelBuilder.Plugins.AddFromType<FileIOPlugin>();
kernelBuilder.Plugins.AddFromType<CalculatorPlugin>();

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
