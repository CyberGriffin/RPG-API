using API5.AI;
using System.Text.RegularExpressions;

namespace API5.Models;

public partial class TaskCheck
{
    public CharacterModel Model = new();
    public string TaskInteraction => GetTaskInteraction();
    public List<int> TaskSuccessConditions => GetTaskSuccessConditions();

    public TaskCheck(CharacterModel model)
    {
        Model = model;
    }

    private string GetTaskInteraction()
    {
        try {
            return Model.TaskInteractions[Model.ActiveTaskId];
        } catch {
            return "No Active Task";
        }
    }

    private List<int> GetTaskSuccessConditions()
    {
        try {
            return  Model.TaskSuccessConditions[Model.ActiveTaskId];
        } catch {
            return new List<int>();
        }
    }

    public async Task<List<Tuple<int, string>>> GetRatings(UserMessage userMessage)
    {
        await userMessage.GetFilteredMessage();

        Task<string> rateRelevancyTask = RateRelevancy(userMessage);
        Task<string> rateConsideranceTask = RateConsiderance(userMessage);
        Task<string> rateToneTask = RateTone(userMessage);
        Task<string> rateLanguageTask = RateLanguage(userMessage);
        
        await Task.WhenAll(rateRelevancyTask, rateConsideranceTask, rateToneTask, rateLanguageTask);

        List<int> ratings = new()
        {
            ExtractScore(rateRelevancyTask.Result),
            ExtractScore(rateConsideranceTask.Result),
            ExtractScore(rateToneTask.Result),
            ExtractScore(rateLanguageTask.Result)
        };

        List<Tuple<int, string>> results = new()
        {
            new(ratings[0], rateRelevancyTask.Result),
            new(ratings[1], rateConsideranceTask.Result),
            new(ratings[2], rateToneTask.Result),
            new(ratings[3], rateLanguageTask.Result)
        };

        return results;
    }

    public int ExtractScore(string rating)
    {
        Console.WriteLine(rating);
        Match match = MyRegex().Match(rating);
        if (match.Success)
        {
            string firstNumber = match.Value;
            return int.Parse(firstNumber);
        }

        return -1;
    }

    public async Task<string> RateRelevancy(UserMessage userMessage)
    {
        string filteredMessage = await userMessage.GetFilteredMessage();

        ICore core = new(
            systemPrompt:
                $"[ INSTRUCTIONS ]\n\n" +
                $"  1. Your job is to rate the user message on a scale of 1-10 based on the following question:\n" +
                $"    - Is the user message related to the current task: {TaskInteraction}? (1-10)\n" +
                $"  2. Compile your rating as a number.\n" +
                $"  3. Respond to the user message with your ratings.\n\n" +
                $"  4. Consider the context of the user message and the task location.\n\n" +
                $"[ END INSTRUCTIONS ]\n\n" +
                $"--------------------------------\n",
            temperature: 0.5f
        );

        (string, string) response = await core.Run(
            $"User message: '{filteredMessage}'\nRating:\nExplain your rating:"
        );

        return response.Item1;
    }

    public async Task<string> RateConsiderance(UserMessage userMessage)
    {
        ICore core = new(
            systemPrompt:
                $"[ INSTRUCTIONS ]\n\n" +
                $"  1. Your job is to rate the user message on a scale of 1-10 based on the following question:\n" +
                $"    - Based on the character's mood ({Model.Mood}) is the user message appropriate? " +
                $"      I.e., strong emotions (angry, super happy, depressed, etc) " +
                $"      will either positively or negatively impact the result of the user message. (1-10)\n" +
                $"  2. Compile your rating as a number.\n" +
                $"  3. Respond to the user message with your ratings.\n\n" +
                $"  4. Consider the context of the user message and the task location.\n\n" +
                $"[ END INSTRUCTIONS ]\n\n" +
                $"--------------------------------\n",
            temperature: 0.5f
        );

        (string, string) response = await core.Run(
            $"User message: '{userMessage.Message}'\nRating:\nExplain your rating:"
        );

        return response.Item1;
    }

    public async Task<string> RateTone(UserMessage userMessage)
    {
        ICore core = new(
            systemPrompt:
                $"[ INSTRUCTIONS ]\n\n" +
                $"  1. Your job is to rate the user message on a scale of 1-10 based on the following question:\n" +
                $"    - Is the user message {Model.TaskTones[Model.ActiveTaskId]}? (1-10)\n" +
                $"  2. Compile your rating as a number.\n" +
                $"  3. Respond to the user message with your ratings.\n\n" +
                $"  4. Consider the context of the user message and the task location.\n\n" +
                $"[ END INSTRUCTIONS ]\n\n" +
                $"--------------------------------\n",
            temperature: 0.5f
        );

        (string, string) response = await core.Run(
            $"User message: '{userMessage.Message}'\nRating:\nExplain your rating:"
        );

        return response.Item1;
    }

    public async Task<string> RateLanguage(UserMessage userMessage)
    {
        string filteredMessage = await userMessage.GetFilteredMessage();

        ICore core = new(
            systemPrompt:
                $"[ INSTRUCTIONS ]\n\n" +
                $"  1. Your job is to rate the user message on a scale of 1-10 based on the following question:\n" +
                $"    - Is the user message in English? (1-10)\n\n" +
                $"  2. Compile your rating as a number.\n" +
                $"  3. Respond to the user message with your ratings.\n\n" +
                $"  4. Consider the context of the user message and the task location.\n\n" +
                $"[ END INSTRUCTIONS ]\n\n" +
                $"--------------------------------\n",
            temperature: 0.5f
        );

        (string, string) response = await core.Run(
            $"User message: '{filteredMessage}'\nRating:\nExplain your rating:"
        );

        return response.Item1;
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex MyRegex();
}
