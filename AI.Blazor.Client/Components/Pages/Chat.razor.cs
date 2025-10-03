using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AI.Blazor.Client.Components.Pages;

public partial class Chat : ComponentBase
{

    protected List<ChatMessage> Messages { get; set; } = new();
    protected string CurrentMessage { get; set; } = string.Empty;
    protected bool IsTyping { get; set; } = false;
    protected ElementReference MessageContainer { get; set; }

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