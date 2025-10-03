using Markdig;

namespace AI.Blazor.Client.Services.Markdown;

public sealed class MarkdigService : IMarkdownService
{
    private readonly MarkdownPipeline pipeline;

    public MarkdigService()
    {
        this.pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        return Markdig.Markdown.ToHtml(markdown, this.pipeline);
    }
}
