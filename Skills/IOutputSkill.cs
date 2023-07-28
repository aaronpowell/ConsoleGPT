using Microsoft.SemanticKernel.Orchestration;

namespace ConsoleGPT.Skills;

internal interface IOutputSkill
{
    /// <summary>
    /// Responds tp the user
    /// </summary>
    public Task<string> Respond(string message, SKContext context);
}
