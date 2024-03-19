namespace API.AI;

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

    public new async Task<string> Run(string query,
    int maxRetries = 3,
    int retryDelayMilliseconds = 1000,
    int taskTimeoutMilliseconds = 2000
    )
    {
        string response = await base.Run(query, maxRetries, retryDelayMilliseconds, taskTimeoutMilliseconds);

        return response;
    }
}
