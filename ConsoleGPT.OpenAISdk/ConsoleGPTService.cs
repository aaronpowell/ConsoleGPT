using Azure;
using Azure.AI.OpenAI;
using ConsoleGPT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

        bool goodbye = false; ChatCompletionsOptions completionsOptions = new()
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

        await WriteAssistantMessage("Beep, boop, I'm .DotNetBot and I'm here to help. If you're done say goodbye.");
        while (!goodbye)
        {
            string? input = await Console.In.ReadLineAsync();

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

            Response<ChatCompletions> completions = await openAIClient.GetChatCompletionsAsync(s.Model, completionsOptions, cancellationToken);

            if (completions.Value.Choices.Count == 0)
            {
                await Console.Out.WriteLineAsync("I'm sorry, I don't know how to respond to that.");
                continue;
            }

            foreach (ChatChoice choice in completions.Value.Choices)
            {
                string content = choice.Message.Content;

                await WriteAssistantMessage(content);

                completionsOptions.Messages.Add(new(ChatRole.Assistant, content));
            }
        }

        lifetime.StopApplication();
    }

    private static async Task WriteAssistantMessage(string content)
    {
        ConsoleColor textColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        await Console.Out.WriteLineAsync(content);
        Console.ForegroundColor = textColor;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}