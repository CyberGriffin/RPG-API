using API.AI;
using API4.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API4.Controllers;

[ApiController]
[Route("[controller]")]
public class OrchestratorController : ControllerBase
{
    public OrchestratorController()
    {}

    [HttpPost, Route("RequestOrchestrator")]
    public async Task<IActionResult> RequestOrchestrator([FromBody] OrchestratorModel data)
    {
        var validationResult = data.IsValid();
        if (validationResult is BadRequestObjectResult)
        {
            return validationResult;
        }

        int tries = 3;
        while (tries > 0) {
            try
            {
                //Create tasks
                var playerIntentTask = PlayerIntent(data);
                var playerToneTask = GetPlayerTone(data);
                var taskSuccessTask = CheckTaskSuccess(data);
                var conversationTopicsTask = ConversationTopics(data);
                await Task.WhenAll(playerIntentTask, playerToneTask, taskSuccessTask, conversationTopicsTask);

                // Get results
                var playerIntentResult = playerIntentTask.Result as OkObjectResult;
                var playerToneResult = playerToneTask.Result as OkObjectResult;
                var taskSuccessResult = taskSuccessTask.Result as OkObjectResult;
                var conversationTopicsResult = conversationTopicsTask.Result as OkObjectResult;

                var intent = "Unknown";
                if (playerIntentResult?.Value is IntentModel intentModel)
                {
                    intent = intentModel.Intent;
                }

                if (taskSuccessResult?.Value is TaskModel taskModel && intent == "Task")
                {
                    bool isSuccess = taskModel.Rating >= 70;
                    bool isCrit = taskModel.Rating >= 90 || taskModel.Rating <= 10;

                    return new OkObjectResult(new { Intent = intent,
                        TaskModel = taskModel,
                        IsSuccess = isSuccess,
                        IsCrit = isCrit,
                        Tone = playerToneResult?.Value is ToneModel toneModel ? toneModel.Tone : "Unknown"
                    });
                }

                else if (conversationTopicsResult?.Value is ConversationTopicModel conversationTopicModel && intent == "ConversationTopics")
                {
                    return new OkObjectResult(new { Intent = intent, ConversationTopicModel = conversationTopicModel });
                }

                else if (intent == "Unknown")
                {
                    if (tries == 1)
                    {
                        return new OkObjectResult(new { Intent = intent });
                    }

                    Console.WriteLine("Retrying...");
                    await Task.Delay(3000 / tries);
                    tries--;
                }

                else
                {
                    Console.WriteLine("Retrying...");
                    await Task.Delay(3000 / tries);
                    tries--;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(3000 / tries);
                tries--;
            }
        }

        return BadRequest("An error occurred while processing the request and failed all retries.");
    }

    [HttpPost, Route("PlayerIntent")]
    public async Task<IActionResult> PlayerIntent([FromBody] OrchestratorModel data)
    {
        var validationResult = data.IsValid();
        if (validationResult is BadRequestObjectResult)
        {
            return validationResult;
        }

        ICore core;

        if (data.HasTask() && data.HasConversationTopics())
        {
            core = new($"The player has this task: '{data.Task}'.\n\n" +
                $"The player also has these possible conversation topics: '{string.Join(", ", data.ConversationTopics!)}'.\n\n" +
                "Determine whether the player's intent was to complete the task or to discuss " +
                "something found in the conversation topics. If they could be attempting both, choose Task.\n\n" +
                "Follow the format of the example responses. DO NOT DEVIATE:\n" +
                "Example response 1: 'Task'\n" +
                "Example response 2: 'ConversationTopics'\n" +
                "Example response 3: 'Unknown'");
        }
        else if (data.HasTask())
        {
            return Ok(new IntentModel { Intent = "Task" });
        }
        else if (data.HasConversationTopics())
        {
            return Ok(new IntentModel { Intent = "ConversationTopics" });
        }
        else
        {
            return BadRequest("No task or conversation topics were provided.");
        }

        string response = await core.Run($"Player Message: {data.PlayerPrompt}");
        return Ok(new IntentModel { Intent = response });
    }

    [HttpPost, Route("GetPlayerTone")]
    public async Task<IActionResult> GetPlayerTone([FromBody] OrchestratorModel data)
    {
        var validationResult = data.IsValid();
        if (validationResult is BadRequestObjectResult)
        {
            return validationResult;
        }

        ICore core = new("What is the tone of the player's message? (Positive, Negative, Neutral)\n\n" +
            "Follow the format of the example responses. DO NOT DEVIATE FROM THE FORMAT:\n" +
            "Example response 1: 'Positive'\n" +
            "Example response 2: 'Negative'\n" +
            "Example response 3: 'Neutral'");
        
        string response = await core.Run($"Player Message: {data.PlayerPrompt}");
        return Ok(new ToneModel { Tone = response });
    }

    [HttpPost, Route("CheckTaskSuccess")]
    public async Task<IActionResult> CheckTaskSuccess([FromBody] OrchestratorModel data)
    {
        var validationResult = data.IsValid();
        if (validationResult is BadRequestObjectResult)
        {
            return validationResult;
        }

        ICore core = new($"The player was given this task: '{data.Task}'.\n\n" +
            "Rate the player's message on a scale of 0-100, where 0 is the worst and 100 is the best.\n" +
            "Consider whether the player's message provides enough information for a person to reasonably understand.\n" +
            "If the player is asking for help with the task, give them a rating of 0.\n\n" +
            "Give the player feedback explaining why you gave them the rating you did and how they can improve to meet the " + 
            "task's requirements in JSON format. DO NOT DEVIATE FROM THE EXAMPLE FORMAT.\n\n" +
            "Example response 1: {\"Rating\": 80, \"Feedback\": \"Your message was clear and concise, however ...\"}\n" +
            "Example response 2: {\"Rating\": 30, \"Feedback\": \"...\"}\n\n");
        
        string response = await core.Run($"Player Message: {data.PlayerPrompt}");

        var ratingAndFeedback = JsonConvert.DeserializeObject<TaskModel>(response);
        return Ok(ratingAndFeedback);
    }

    [HttpPost, Route("ConversationTopics")]
    public async Task<IActionResult> ConversationTopics([FromBody] OrchestratorModel data)
    {
        var validationResult = data.IsValid();
        if (validationResult is BadRequestObjectResult)
        {
            return validationResult;
        }

        // ICore core = new($"Respond to the player using the following conversation topics for guidance: " +
        //     $"'{string.Join(",\n", data.ConversationTopics!)}'.\n\n" + 
        //     "Do not add any new information. Drop words that are surrounded perenthesis and end with a colon, i.e., (word):.\n" +
        //     "The perenthesis and colon helps you select the proper response based on the player's attitude. I.e., (If Rude):.\n" +
        //     "If the player's message is not related to the conversation topics, " +
        //     "respond with 'Sorry, I'm not sure how to answer that.'\n\n" +
        //     "Follow the format of the example responses. DO NOT DEVIATE FROM THE FORMAT:\n" +
        //     "Example response 1: Hello!\n" +
        //     "Example response 2: Sorry, I'm not sure how to answer that.\n");

        // ICore core = new($"Match the player's message with one of the following topics: " +
        //     $"'{string.Join(";\n", data.ConversationTopics!)}'\n\n" +
        //     "The format of the topics is '(condition to be met): response'.\n" +
        //     "Respond with JSON format. DO NOT DEVIATE FROM THE FORMAT:\n" +
        //     "Example response 1 using topic '(Is greeting): Hello!': {\"Response\": \"Hello!\"}\n" +
        //     "Example response 2 using topic '(condition to be met) response: {\"Response\": \"response\"}\n");

        ICore core = new($"You're given a list of topics formated as such: '(condition): response'.\n" + 
            "Select the response that best fits the player's message. If the player's message doesn't match any of the topics, " +
            "respond with 'Sorry, I'm not sure how to answer that.'\n\n" +
            $"List of topics: {string.Join("\n", data.ConversationTopics!)}\n\n" +
            "After selecting the response, drop the condition and colon from the response.\n\n" +
            "Follow the JSON format of the example responses. DO NOT DEVIATE FROM THE FORMAT:\n" +
            "Example response 1 using topic '(Is greeting): Hello!': {\"Response\": \"Hello!\"}\n" +
            "Example response 2 using topic '(condition) response: {\"Response\": \"response\"}\n"
            );

        string response = await core.Run($"Player Message: {data.PlayerPrompt}");  
        return Ok(JsonConvert.DeserializeObject<ConversationTopicModel>(response));
    }
}
