using AI.Blazor.Client.Services.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AI.Blazor.Client.Components.Pages;

public partial class Chat : ComponentBase
{
    [Inject]
    private IChatService ChatService { get; set; } = default!;

    [Inject]
    private ILogger<Chat> Logger { get; set; } = default!;

    protected List<ChatMessage> Messages { get; set; } = new();
    protected string CurrentMessage { get; set; } = string.Empty;
    protected bool IsTyping { get; set; } = false;
    protected ElementReference MessageContainer { get; set; }
    protected string? ErrorMessage { get; set; }

    protected override void OnInitialized()
    {
        var welcomeMessage = "Hello! I'm your AI assistant. How can I help you today?";
        this.Messages.Add(new ChatMessage
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
        this.Messages.Add(new ChatMessage
        {
            Text = userMessage,
            IsUser = true,
            Timestamp = DateTime.Now
        });

        this.CurrentMessage = string.Empty;
        this.IsTyping = true;
        this.ErrorMessage = null;

        try
        {
            var response = await this.ChatService.GetResponse(userMessage);

            this.Messages.Add(new ChatMessage
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
            
            this.Messages.Add(new ChatMessage
            {
                Text = this.ErrorMessage,
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            this.IsTyping = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(this.CurrentMessage))
        {
            await this.SendMessage();
        }
    }
}

public class ChatMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; }
}