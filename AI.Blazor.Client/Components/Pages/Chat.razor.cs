using AI.Blazor.Client.Services.Chat;
using AI.Blazor.Client.Services.Welcome;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AI.Blazor.Client.Components.Pages;

public partial class Chat : ComponentBase
{
    [Inject]
    private IChatService ChatService { get; set; } = default!;

    [Inject]
    private IWelcomeService WelcomeService { get; set; } = default!;

    [Inject]
    private ILogger<Chat> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected List<ChatMessageViewModel> Messages { get; set; } = new();
    protected string CurrentMessage { get; set; } = string.Empty;
    protected bool IsTyping { get; set; } = false;
    protected bool IsStreaming { get; set; } = false;
    protected bool IsStreamingEnabled { get; set; } = true;
    protected bool IsProcessing => this.IsTyping || this.IsStreaming;
    protected ElementReference MessageContainer { get; set; }
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await this.LoadWelcomeMessage();
    }

    private async Task LoadWelcomeMessage()
    {       
        var welcomeMessage = await this.WelcomeService.GenerateWelcomeMessage();

        this.Messages.Add(new ChatMessageViewModel
        {
            Text = welcomeMessage,
            IsUser = false,
            Timestamp = DateTime.Now
        });
    }

    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(this.CurrentMessage))
            return;

        var userMessage = this.CurrentMessage.Trim();
        this.Messages.Add(new ChatMessageViewModel
        {
            Text = userMessage,
            IsUser = true,
            Timestamp = DateTime.Now
        });

        await this.InvokeAsync(this.StateHasChanged);
        await this.ScrollToBottom();

        this.CurrentMessage = string.Empty;
        this.ErrorMessage = null;

        if (this.IsStreamingEnabled)
        {
            await this.SendStreamingMessage(userMessage);
        }
        else
        {
            await this.SendNonStreamingMessage(userMessage);
        }
    }

    private async Task SendNonStreamingMessage(string userMessage)
    {
        this.IsTyping = true;

        try
        {
            var response = await this.ChatService.GetResponse(userMessage);

            this.Messages.Add(new ChatMessageViewModel
            {
                Text = response,
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting chat response");
            this.ErrorMessage = "Sorry, I encountered an error. Please try again.";
            
            this.Messages.Add(new ChatMessageViewModel
            {
                Text = this.ErrorMessage,
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            this.IsTyping = false;
            await this.InvokeAsync(this.StateHasChanged);
            await this.ScrollToBottom();
        }
    }

    private async Task SendStreamingMessage(string userMessage)
    {
        this.IsStreaming = true;

        var aiMessage = new ChatMessageViewModel
        {
            Text = string.Empty,
            IsUser = false,
            Timestamp = DateTime.Now
        };

        this.Messages.Add(aiMessage);
        await this.InvokeAsync(this.StateHasChanged);
        await this.ScrollToBottom();

        try
        {
            var lastUpdate = DateTime.UtcNow;
            const int throttleMs = 100; // Update UI every 50ms maximum

            await foreach (var chunk in this.ChatService.GetStreamingResponse(userMessage))
            {
                aiMessage.Text += chunk;

                var elapsed = (DateTime.UtcNow - lastUpdate).TotalMilliseconds;
                if (elapsed >= throttleMs)
                {
                    await this.InvokeAsync(this.StateHasChanged);
                    await this.ScrollToBottom();
                    lastUpdate = DateTime.UtcNow;
                }
            }

            // Final update to ensure all chunks are displayed
            await this.InvokeAsync(this.StateHasChanged);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting streaming chat response");
            this.ErrorMessage = "Sorry, I encountered an error. Please try again.";

            aiMessage.Text = this.ErrorMessage;
        }
        finally
        {
            this.IsStreaming = false;
            await this.InvokeAsync(this.StateHasChanged);
        }
    }    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(this.CurrentMessage))
        {
            await this.SendMessage();
        }
    }

    private async Task ScrollToBottom()
    {
        await this.JSRuntime.InvokeVoidAsync("scrollToBottom", this.MessageContainer);
    }
}

public class ChatMessageViewModel
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; }
}