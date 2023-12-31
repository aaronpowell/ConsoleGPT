﻿using ConsoleGPT;
using ConsoleGPT.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

hostBuilder.ConfigureServices((context, services) =>
{
    services.Configure<Settings>(context.Configuration.GetSection("settings"));
    services.AddSingleton(provider =>
    {
        Settings settings = provider.GetRequiredService<IOptions<Settings>>().Value;

        KernelBuilder kernelBuilder = new();

        if (settings.Type == OpenAIType.Azure)
        {
            kernelBuilder.WithAzureChatCompletionService(settings.Model, settings.Endpoint!, settings.Key);
        }
        else
        {
            kernelBuilder.WithOpenAIChatCompletionService(settings.Model, settings.Key, settings.OrgId!);
        }

        return kernelBuilder.Build();
    });

    services.AddHostedService<ConsoleGPTService>();

    // Custom skills
    services.AddSingleton<IInputSkill, ConsoleInputSkill>();
    services.AddSingleton<IOutputSkill, ConsoleOutputSkill>();
    services.AddSingleton<ChatSkill>();

    // Built-in skills
    services.AddSingleton<TextSkill>();
});

await hostBuilder.Build().RunAsync();