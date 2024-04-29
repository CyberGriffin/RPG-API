using Azure.AI.OpenAI;

namespace API5.AI;

public class Core
{
    protected string _systemPrompt;
    protected readonly OpenAIClient _client;
    protected ChatCompletionsOptions _options;
    private string OPENAI_API_KEY { get; } = Environment.GetEnvironmentVariable("RPGMAKER_OPENAI_API_KEY") ?? 
        throw new Exception("RPGMAKER_OPENAI_API_KEY environment variable not set.");
    
    protected Core(string systemPrompt,
        float temperature = 0.9f,
        int maxTokens = 150,
        float frequencyPenalty = 0.0f,
        float presencePenalty = 0.6f,
        string deploymentName = "gpt-3.5-turbo"
    )
    {
        _systemPrompt = systemPrompt;
        _client = new OpenAIClient(OPENAI_API_KEY);
        _options = new ChatCompletionsOptions
        {
            Temperature = temperature,
            MaxTokens = maxTokens,
            FrequencyPenalty = frequencyPenalty,
            PresencePenalty = presencePenalty,
            DeploymentName = deploymentName
        };

        _options.Messages.Add(new ChatRequestSystemMessage(systemPrompt));
    }

    protected async Task<string> Run(string query,
        int maxRetries = 3,
        int retryDelayMilliseconds = 1000,
        int taskTimeoutMilliseconds = 2000
    )
    {
        if (string.IsNullOrEmpty(query.Trim()))
        {
            return "";
        }

        // Append the user's query to the list of messages to send to the API
        _options.Messages.Add(new ChatRequestUserMessage(query));

        int currentRetry = 0;
        while (currentRetry <= maxRetries)
        {
            try
            {
                var responseTask = _client.GetChatCompletionsStreamingAsync(_options);
                using var cts = new CancellationTokenSource();
                var completedTask = await Task.WhenAny(responseTask, Task.Delay(taskTimeoutMilliseconds, cts.Token));

                if (completedTask == responseTask)
                {
                    var response = responseTask.Result;
                    string fullresponse = "";
                    await foreach (var update in response) {
                        if (update.ContentUpdate != null) {
                            fullresponse += update.ContentUpdate.ToString();
                            // Callback functions go here, if any
                        }
                    }

                    _options.Messages.Add(new ChatRequestAssistantMessage(fullresponse));
                    return fullresponse;
                } else
                {
                    cts.Cancel();
                    throw new TaskCanceledException();
                }
            } catch (TaskCanceledException tce) when (tce.CancellationToken.IsCancellationRequested)
            {
                ++currentRetry;
                Backoff(retryDelayMilliseconds, currentRetry);
            } catch (Exception ex)
            {
                return ex.Message;
            }
        }

        return "";
    }

    private protected static async void Backoff(int retryDelayMilliseconds, int currentRetry)
    {
        int backoff = (int)((Math.Pow(2, currentRetry) - 1) * retryDelayMilliseconds);
        await Task.Delay(backoff);
    }

    protected void UpdateSystemPrompt(string prompt)
    {
        _systemPrompt += prompt;

        _options.Messages.Clear();
        _options.Messages.Add(new ChatRequestSystemMessage(_systemPrompt));
    }
}
