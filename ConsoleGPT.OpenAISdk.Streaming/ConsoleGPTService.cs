using Azure;
using Azure.AI.OpenAI;
using ConsoleGPT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;

internal class ConsoleGPTService : IHostedService
{
    private readonly IHostApplicationLifetime lifetime;
    private readonly OpenAIClient openAIClient;
    private readonly IOptions<Settings> settings;

    public ConsoleGPTService(IHostApplicationLifetime lifetime, OpenAIClient openAIClient, IOptions<Settings> settings)
    {
        this.lifetime = lifetime;
        this.openAIClient = openAIClient;
        this.settings = settings;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Settings s = settings.Value;

        bool goodbye = false;
        ChatCompletionsOptions completionsOptions = new()
        {
            MaxTokens = s.MaxTokens,
            Temperature = s.Temperature,
            FrequencyPenalty = s.FrequencyPenalty,
            PresencePenalty = s.PresencePenalty,
            Messages =
            {
                new(ChatRole.System, s.SystemPrompt)
            }
        };

        WriteAssistantMessage("Beep, boop, I'm .DotNetBot and I'm here to help. If you're done say goodbye.", newLine: true);
        while (!goodbye)
        {
            string? input = await Console.In.ReadLineAsync(cancellationToken);

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            if (input.Equals("goodbye", StringComparison.OrdinalIgnoreCase))
            {
                goodbye = true;
                continue;
            }

            completionsOptions.Messages.Add(new(ChatRole.User, input));

            Response<StreamingChatCompletions> completions = await openAIClient.GetChatCompletionsStreamingAsync(s.Model, completionsOptions, cancellationToken);

            IAsyncEnumerable<StreamingChatChoice> choices = completions.Value.GetChoicesStreaming(cancellationToken);

            await foreach (StreamingChatChoice choice in choices)
            {
                IAsyncEnumerable<ChatMessage> message = choice.GetMessageStreaming(cancellationToken);

                StringBuilder completeMessage = new();

                await foreach (ChatMessage m in message)
                {
                    string content = m.Content;

                    WriteAssistantMessage(content, newLine: false);

                    completeMessage.Append(content);

                    // Simulate some delay in the network
                    await Task.Delay(200, cancellationToken);
                }
                Console.WriteLine();

                completionsOptions.Messages.Add(new(ChatRole.Assistant, completeMessage.ToString()));

            }
        }

        lifetime.StopApplication();
    }

    private static void WriteAssistantMessage(string content, bool newLine = true)
    {
        ConsoleColor textColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        if (newLine)
        {
            Console.Out.WriteLine(content);
        }
        else
        {
            Console.Out.Write(content);
        }
        Console.ForegroundColor = textColor;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}