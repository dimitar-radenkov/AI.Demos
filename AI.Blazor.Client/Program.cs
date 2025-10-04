using AI.Shared.Settings;
using AI.Blazor.Client.Components;
using AI.Blazor.Client.Services.Chat;
using AI.Blazor.Client.Services.Markdown;
using AI.Blazor.Client.Services.Welcome;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure settings
var llmOptions = builder.Configuration.GetSection(LlmSettings.SectionName).Get<LlmSettings>();
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection(ChatSettings.SectionName));
builder.Services.Configure<WelcomeSettings>(builder.Configuration.GetSection("WelcomeSettings"));

// Register Semantic Kernel services
builder.Services.AddOpenAIChatCompletion(
    modelId: llmOptions!.Model,
    apiKey: llmOptions.ApiKey,
    endpoint: new Uri($"{llmOptions.BaseUrl}/v1"));

builder.Services.AddTransient<Kernel>();

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
