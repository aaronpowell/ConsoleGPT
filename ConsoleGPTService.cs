using ConsoleGPT.Skills;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Core;

namespace ConsoleGPT;

internal class ConsoleGPTService : IHostedService
{
    private readonly IKernel kernel;
    private readonly IHostApplicationLifetime lifeTime;
    private readonly IDictionary<string, ISKFunction> inputSkillFunctions;
    private readonly IDictionary<string, ISKFunction> chatSkillFunctions;
    private readonly IDictionary<string, ISKFunction> outputSkillFunctions;
    private readonly IDictionary<string, ISKFunction> textSkillFunctions;

    public ConsoleGPTService(
        IKernel kernel,
        IHostApplicationLifetime lifeTime,
        ChatSkill chatSkill,
        IInputSkill inputSkill,
        IOutputSkill outputSkill,
        TextSkill textSkill)
    {
        this.kernel = kernel;
        this.lifeTime = lifeTime;

        inputSkillFunctions = kernel.ImportSkill(inputSkill);
        chatSkillFunctions = kernel.ImportSkill(chatSkill);
        outputSkillFunctions = kernel.ImportSkill(outputSkill);
        textSkillFunctions = kernel.ImportSkill(textSkill);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await kernel.RunAsync(
            "Beep, boop, I'm .DotNetBot and I'm here to help. If you're done say goodbye.",
            cancellationToken,
            outputSkillFunctions[nameof(IOutputSkill.RespondAsync)]
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            ISKFunction[] pipeline = {
                inputSkillFunctions[nameof(IInputSkill.ListenAsync)],
                textSkillFunctions[nameof(TextSkill.TrimStart)],
                textSkillFunctions[nameof(TextSkill.TrimEnd)],
                chatSkillFunctions[nameof(ChatSkill.PromptAsync)],
                outputSkillFunctions[nameof(IOutputSkill.RespondAsync)]
            };

            await kernel.RunAsync(pipeline);

            SKContext goodbyeContext = await kernel.RunAsync(cancellationToken, inputSkillFunctions[nameof(IInputSkill.IsGoodbyeAsync)]);
            if (bool.TryParse(goodbyeContext.Result, out bool result) && result)
            {
                await kernel.RunAsync(cancellationToken, chatSkillFunctions[nameof(ChatSkill.LogChatHistoryAsync)]);
                break;
            }
        }

        lifeTime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
