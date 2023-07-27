using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConsoleGPT.Skills;

internal class ConsoleInputSkill : IInputSkill
{
    private bool _isGoodbye = false;

    [SKFunction]
    public Task<string> Listen(SKContext context)
    {
        return Task.Run(() => {
            string? line = "";

            while (string.IsNullOrWhiteSpace(line))
            {
                line = Console.ReadLine();
            }

            if (line.ToLower().StartsWith("goodbye"))
                _isGoodbye = true;

            return line;
        });
    }

    [SKFunction]
    public Task<string> Respond(string message, SKContext context)
    {
        return Task.Run(() => {
            WriteAIResponse(message);
            return message;
        });
    }

    [SKFunction]
    public Task<string> IsGoodbye(SKContext context)
    {
        return Task.FromResult(_isGoodbye ? "true" : "false");
    }

    /// <summary>
    /// Write a response to the console in green.
    /// </summary>
    private static void WriteAIResponse(string response)
    {
        // Write the response in Green, then revert the console color
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(response);
        Console.ForegroundColor = oldColor;
    }
}
