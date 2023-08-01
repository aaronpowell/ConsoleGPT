using Microsoft.SemanticKernel.Orchestration;

namespace ConsoleGPT.Skills;

internal interface IInputSkill
{
    /// <summary>
    /// Gets input from the user
    /// </summary>
    public Task<string> ListenAsync(SKContext context);

    /// <summary>
    /// Gets if Listen function detected goodbye from the user
    /// </summary>
    public Task<bool> IsGoodbyeAsync(SKContext context);
}
