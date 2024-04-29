using API5.Models;

namespace API5.AI;

public class ICore : Core
{
    public ICore(string systemPrompt = "",
        float temperature = 0.9f,
        int maxTokens = 150,
        float frequencyPenalty = 0.0f,
        float presencePenalty = 0.6f,
        string deploymentName = "gpt-3.5-turbo"
    ) : base(systemPrompt,
        temperature,
        maxTokens,
        frequencyPenalty,
        presencePenalty,
        deploymentName
    )
    {}

    public async Task<(string, string)> Run(string query, string conversationHistory = "",
    int maxRetries = 3,
    int retryDelayMilliseconds = 1000,
    int taskTimeoutMilliseconds = 2000
    )
    {
        conversationHistory += $"\nPlayer: {query}\n";
        UpdateSystemPrompt(conversationHistory);

        string response = await base.Run(query, maxRetries, retryDelayMilliseconds, taskTimeoutMilliseconds);

        return (response, conversationHistory);
    }

    public string GetSystemPrompt()
    {
        return _systemPrompt;
    }
}

static class ICoreUtils
{
    public static async Task<string> Filter(string message)
    {   
        ICore core = new(
            systemPrompt: "Your job is to rewrite a message to be more friendly and considerate.\n" +
                "Respond with the rewritten message as such: '<your message>'\n",
            temperature: 0.5f
        );
        // Filter out any inappropriate content
        (string, string) response = await core.Run($"User message: '{message}'\nFiltered message:");

        return response.Item1;
    }
}