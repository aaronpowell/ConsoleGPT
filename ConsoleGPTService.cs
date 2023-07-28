using ConsoleGPT.Skills;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConsoleGPT;

internal class ConsoleGPTService : IHostedService
{
    private readonly IKernel kernel;
    private readonly IHostApplicationLifetime lifeTime;
    private readonly IDictionary<string, ISKFunction> inputSkillFunctions;
    private readonly IDictionary<string, ISKFunction> chatSkillFunctions;
    private readonly IDictionary<string, ISKFunction> outputSkillFunctions;

    public ConsoleGPTService(
        IKernel kernel,
        IHostApplicationLifetime lifeTime,
        ChatSkill chatSkill,
        IInputSkill inputSkill,
        IOutputSkill outputSkill)
    {
        this.kernel = kernel;
        this.lifeTime = lifeTime;

        inputSkillFunctions = kernel.ImportSkill(inputSkill);
        chatSkillFunctions = kernel.ImportSkill(chatSkill);
        outputSkillFunctions = kernel.ImportSkill(outputSkill);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await kernel.RunAsync(
            "Beep, boop, I'm .DotNetBot and I'm here to help. If you're done say goodbye.",
            cancellationToken,
            outputSkillFunctions[nameof(IOutputSkill.Respond)]
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            ISKFunction[] pipeline = {
                inputSkillFunctions[nameof(IInputSkill.Listen)],
                chatSkillFunctions[nameof(ChatSkill.Prompt)],
                outputSkillFunctions[nameof(IOutputSkill.Respond)]
            };

            await kernel.RunAsync(pipeline);

            SKContext goodbyeContext = await kernel.RunAsync(cancellationToken, inputSkillFunctions[nameof(IInputSkill.IsGoodbye)]);
            if (goodbyeContext.Result == "true")
            {
                await kernel.RunAsync(cancellationToken, chatSkillFunctions[nameof(ChatSkill.LogChatHistory)]);
                break;
            }
        }

        lifeTime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
