using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Options;

namespace ConsoleGPT.Skills;

internal class ChatSkill
{
    private readonly ChatRequestSettings chatRequestSettings;
    private readonly IChatCompletion chatCompletion;
    private readonly ChatHistory chatHistory;

    public ChatSkill(IKernel kernel, IOptions<Settings> settings)
    {
        Settings s = settings.Value;
        chatRequestSettings = new()
        {
            MaxTokens = s.MaxTokens,
            Temperature = s.Temperature,
            FrequencyPenalty = s.FrequencyPenalty,
            PresencePenalty = s.PresencePenalty,
            TopP = s.TopP
        };

        chatCompletion = kernel.GetService<IChatCompletion>();
        chatHistory = chatCompletion.CreateNewChat(s.SystemPrompt);
    }

    [SKFunction, SKName(nameof(PromptAsync))]
    public async Task<string> PromptAsync(string prompt)
    {
        string reply = string.Empty;
        try
        {
            // Add the question as a user message to the chat history, then send everything to OpenAI.
            // The chat history is used as context for the prompt
            chatHistory.AddUserMessage(prompt);
            IReadOnlyList<IChatResult> completion =
                await chatCompletion.GetChatCompletionsAsync(chatHistory, chatRequestSettings);

            if (!completion.Any())
                throw new AIException(AIException.ErrorCodes.NoResponse, "No completion results returned from OpenAI.");

            foreach (IChatResult result in completion)
            {
                // Add the completion result as an assistant message to the chat history.
                ChatMessageBase message = await result.GetChatMessageAsync();
                chatHistory.AddAssistantMessage(message.Content);

                reply += Environment.NewLine + message.Content;
            }
        }
        catch (AIException aiex)
        {
            // Reply with the error message if there is one
            reply = $"OpenAI returned an error ({aiex.Message}). Please try again.";
        }

        return reply;
    }

    [SKFunction, SKName(nameof(LogChatHistoryAsync))]
    public async Task LogChatHistoryAsync()
    {
        await Console.Out.WriteLineAsync();
        await Console.Out.WriteLineAsync("Chat history:");
        await Console.Out.WriteLineAsync();

        // Log the chat history including system, user and assistant (AI) messages
        foreach (ChatMessageBase message in chatHistory.Messages)
        {
            // Depending on the role, use a different color
            AuthorRole role = message.Role;

            (string label, ConsoleColor colour) = role switch
            {
                _ when role == AuthorRole.System =>
                    ("System:    ", ConsoleColor.Blue),
                _ when role == AuthorRole.User =>
                    ("User:      ", ConsoleColor.Yellow),
                _ when role == AuthorRole.Assistant =>
                    ("Assistant: ", ConsoleColor.Green),
                _ => ("Unknown:  ", ConsoleColor.Magenta)
            };

            Console.ForegroundColor = colour;
            await Console.Out.WriteLineAsync($"{label}{message.Content}");
        }
    }
}
