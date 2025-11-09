using AI.Agents;
using AI.Core.Infrastructure;
using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using PDFtoImage;
using SkiaSharp;
using System.ClientModel;
using System.Text.Json;

var invoiceAgentSettings = new AgentSettings
{
    BaseUrl = "http://127.0.0.1:55443",
    ApiKey = "not-needed",
    Model = "qwen2.5-vl-32b-instruct",
    SystemPrompt =
    [
        "You are an expert invoice processing agent specialized in extracting relevant information from invoices and generating structured data outputs.",
        "",
        "Your primary focus is to accurately identify and extract key invoice details such as invoice number, date, vendor information, line items, totals, and tax information.",
        "",
        "Extraction Process:",
        "1. Analyze the invoice content thoroughly",
        "2. Identify and extract key fields",
        "3. Structure the extracted data in a clear format",
        "",
        "Key Fields to Extract:",
        "- Invoice Number",
        "- Invoice Date",
        "- Vendor Name",
        "- Tax Amount",
        "",
        "Remember:",
        "- Ensure accuracy in extraction"
    ],
};
var options = Options.Create(invoiceAgentSettings);
var invoiceAgent = new InvoiceAgent(options);

var agentResponse = await invoiceAgent.ExecuteAsync(
    "Please extract the invoice number, date, vendor name, and total amount from the attached invoice image.");

if (agentResponse.IsSuccess)
{
    Console.WriteLine(agentResponse);
}
else
{
    Console.WriteLine($"Error: {agentResponse.ErrorMessage}");
}

public sealed record InvoiceModel(
    string InvoiceNumber,
    string InvoiceDate,
    string VendorName,
    decimal TotalAmount);

public sealed record InvoiceAgentResult : OperationResult<InvoiceModel, InvoiceAgentResult>;

public sealed class InvoiceAgent : IAgent<InvoiceAgentResult>
{
    private readonly AIAgent agent;

    public InvoiceAgent(IOptions<AgentSettings> options)
    {
        var settings = options.Value;
        
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(InvoiceModel));

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(schema: schema)
        };

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "Invoice-Agent",
                    Instructions = settings.GetSystemPrompt(),
                    ChatOptions = chatOptions
                });
    }

    public async Task<InvoiceAgentResult> ExecuteAsync(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceFilePath = @"c:\Users\dimit\Downloads\Invoice-0000000140-ORIGINAL.pdf";                   
            var imageBytes = await PdfFileHelper.ConvertPdfToImageBytesAsync(invoiceFilePath, cancellationToken);

            var message = new ChatMessage(
                ChatRole.User, 
                [
                    new TextContent(userRequest),
                    new DataContent(new ReadOnlyMemory<byte>(imageBytes), "image/png")
                ]);

            var response = await this.agent.RunAsync(message, cancellationToken: cancellationToken);
            var invoiceModel = response.Deserialize<InvoiceModel>(JsonSerializerOptions.Web);
            
            return invoiceModel is null
                ? InvoiceAgentResult.Failure("Failed to parse invoice data from agent response")
                : InvoiceAgentResult.Success(invoiceModel);
        }
        catch (JsonException ex)
        {
            return InvoiceAgentResult.Failure($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return InvoiceAgentResult.Failure($"Unexpected error: {ex.Message}");
        }
    }
}

public static class PdfFileHelper 
{
    public static async Task<byte[]> ConvertPdfToImageBytesAsync(
        string pdfFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfFilePath))
            throw new ArgumentException("PDF file path cannot be null or empty.", nameof(pdfFilePath));

        if (!File.Exists(pdfFilePath))
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}", pdfFilePath);

        using var pdfStream = File.OpenRead(pdfFilePath);
        using var bitmap = Conversion.ToImage(pdfStream);
        using var memoryStream = new MemoryStream();

        bitmap.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
        return memoryStream.ToArray();
    }
}