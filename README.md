# ConsoleGPT

## Description

This is a demo application showing how you can build a ChatGPT like experience using [Azure OpenAI Service](https://azure.microsoft.com/services/cognitive-services/openai-text-generation/).

There are three demo applications:

- [ConsoleGPT.OpenAISdk](./ConsoleGPT.OpenAISdk) - This is a demo application using the [Azure OpenAI SDK](https://learn.microsoft.com/dotnet/api/overview/azure/ai.openai-readme?view=azure-dotnet-preview) directly
- [ConsoleGPT.OpenAISdk.Streaming](./ConsoleGPT.OpenAISdk.Streaming) - Same as the first demo but uses a streaming response (with simulated network latency)
- [ConsoleGPT.SemanticKernelDemo](./ConsoleGPT.SemanticKernelDemo) - This is a demo application using the [Semantic Kernel](https://learn.microsoft.com/semantic-kernel/overview/) to orchestarte the Azure OpenAI Service

_Note: While these demos refer to Azure OpenAI Service, OpenAI can also be used directly._

## Getting Started

Clone the repository:

```bash
git clone https://github.com/aaronpowell/ConsoleGPT.git
```

Open in Visual Studio, VS Code, or your favourite editor.

You need to add your connection information to either Azure OpenAI Service of OpenAI to `appsettings.json`. Here's a sample for Azure OpenAI Service:

```json
{
  "settings": {
    "model": "chat",
    "endpoint": "https://<your resource>.openai.azure.com/",
    "key": "<your key>",
    "type": "azure"
  }
}
```

_Note: For Azure OpenAI Service the `model` is the name of the model created from the foundation model. For OpenAI it would be the GPT model such as `gpt-3.5-turbo`._

## Running the Demo

Set the application you want to run as the Startup Project and start a debugging session. Alternatively, navigate to the folder on the command line and run `dotnet run`.

## License

MIT

## Credits

Thanks to [Jim Bennett](https://github.com/jimbobbennett) for the [original demo inspiration](https://github.com/jimbobbennett/console-gpt).
