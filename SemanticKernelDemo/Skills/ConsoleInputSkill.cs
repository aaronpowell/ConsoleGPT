using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConsoleGPT.Skills;

internal class ConsoleInputSkill : IInputSkill
{
    private bool _isGoodbye = false;

    [SKFunction, SKName(nameof(ListenAsync))]
    public Task<string> ListenAsync(SKContext context)
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

    [SKFunction, SKName(nameof(IsGoodbyeAsync))]
    public Task<bool> IsGoodbyeAsync(SKContext context)
    {
        return Task.FromResult(_isGoodbye);
    }
}
