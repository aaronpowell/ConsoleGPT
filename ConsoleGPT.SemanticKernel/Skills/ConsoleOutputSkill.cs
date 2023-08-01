using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConsoleGPT.Skills;

internal class ConsoleOutputSkill : IOutputSkill
{
    [SKFunction, SKName(nameof(RespondAsync))]
    public async Task<string> RespondAsync(string message, SKContext context)
    {
        await WriteAIResponse(message);
        return message;
    }

    /// <summary>
    /// Write a response to the console in green.
    /// </summary>
    private static async Task WriteAIResponse(string response)
    {
        // Write the response in Green, then revert the console color
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        await Console.Out.WriteLineAsync(response);
        Console.ForegroundColor = oldColor;
    }
}