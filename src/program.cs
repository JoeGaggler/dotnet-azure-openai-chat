using Azure.AI.OpenAI;
using static System.Console;

var envKeyName = "AZURE_OPENAI_KEY";
var envUrlName = "AZURE_OPENAI_URL";
var envDeploymentName = "AZURE_OPENAI_DEPLOYMENT";

var deploymentName = Environment.GetEnvironmentVariable(envDeploymentName) ?? "gpt-4-turbo";

var url = Environment.GetEnvironmentVariable(envUrlName);
if (String.IsNullOrEmpty(url))
{
    WriteLine($"Please set the environment variable '{envUrlName}' to the Azure OpenAI endpoint, e.g. 'https://youraccount.openai.azure.com/'");
    return 1;
}

var secret = Environment.GetEnvironmentVariable(envKeyName);
if (String.IsNullOrEmpty(secret))
{
    WriteLine($"Please set the environment variable '{envKeyName}' to the Azure OpenAI key.");
    return 1;
}

var client = new OpenAIClient(new Uri(url), new Azure.AzureKeyCredential(secret));

var chatCompletionOptions = new ChatCompletionsOptions()
{
    DeploymentName = deploymentName,
    ChoiceCount = 1
};

var messages = chatCompletionOptions.Messages;

WriteLine("Enter the system prompt, or hit {Enter} for default.");
WriteLine();
var systemPrompt = ReadLine();
if (String.IsNullOrEmpty(systemPrompt)) { systemPrompt = "You are a helpful chatbot. Please respond to all of the users questions in the most helpful manner."; }
messages.Add(new ChatRequestSystemMessage(systemPrompt));

WriteLine();
WriteLine("Enter the user prompt, then hit {Enter}. You may continue the conversation, or hit {Enter} to exit.");

while (true)
{
    Write("> ");
    var nextUserMessage = ReadLine();
    if (String.IsNullOrEmpty(nextUserMessage)) { break; }
    messages.Add(new ChatRequestUserMessage(nextUserMessage));

    var nextAssistantMessage = String.Empty;
    await foreach (var streamUpdate in await client.GetChatCompletionsStreamingAsync(chatCompletionOptions, CancellationToken.None))
    {
        if (streamUpdate.ChoiceIndex is not 0) continue;
        Write(streamUpdate.ContentUpdate);
        nextAssistantMessage += streamUpdate.ContentUpdate;
    }
    WriteLine();
    messages.Add(new ChatRequestAssistantMessage(nextAssistantMessage));
}

return 0;
