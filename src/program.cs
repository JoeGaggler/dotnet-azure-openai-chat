using Azure.AI.OpenAI;
using static System.Console;

var envKeyName = "AZURE_OPENAI_KEY";
var envUrlName = "AZURE_OPENAI_URL";

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
    DeploymentName = "gpt-4-32k",
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

    ChatCompletions response = await client.GetChatCompletionsAsync(chatCompletionOptions, CancellationToken.None);
    var nextAssistantMessage = response.Choices[0].Message.Content;
    WriteLine();
    WriteLine(nextAssistantMessage);
    messages.Add(new ChatRequestAssistantMessage(nextAssistantMessage));
}

return 0;
