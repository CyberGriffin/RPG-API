using API5.AI;
using API5.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API5.Controllers;

[ApiController]
[Route("[controller]")]
public class CharacterController : ControllerBase
{
    public CharacterController() {}

    [HttpPost]
    public async Task<AIResponse> Chat([FromBody] UserMessage userMessage)
    {
        // Json to object
        CharacterModel model = JsonConvert.DeserializeObject<CharacterModel>(userMessage.CharacterJson)!;
        TaskCheck taskCheck = new(model);

        // Get the user's ratings for the task
        List<Tuple<int, string>> ratingResponse = await taskCheck.GetRatings(userMessage);

        bool wasRelevant = ratingResponse[0].Item1 >= model.TaskSuccessConditions[model.ActiveTaskId][0];
        bool wasConsiderate = ratingResponse[1].Item1 >= model.TaskSuccessConditions[model.ActiveTaskId][1];
        bool wasCorrectTone = ratingResponse[2].Item1 >= model.TaskSuccessConditions[model.ActiveTaskId][2];
        bool wasCorrectLanguage = ratingResponse[3].Item1 >= model.TaskSuccessConditions[model.ActiveTaskId][3];

        bool wasSuccessful = wasRelevant && wasConsiderate && wasCorrectTone && wasCorrectLanguage;

        // Provide instructions for the NPC's response based on the ratings
        string NPCResponseInstructions;

        NPCResponseInstructions = wasRelevant ?
            "You need to " + (wasSuccessful ? $"Accept" : $"Debate") +
                $" the user's request for the task: {model.TaskInteractions[model.ActiveTaskId]}.\n" :
            $"";
            // $"The user was attempting this task: {model.TaskInteractions[model.ActiveTaskId]} and was " +
            //     (wasSuccessful ? "successful.\n" : "unsuccessful.\n") :
            // "";
        
        NPCResponseInstructions += wasConsiderate ?
            $"The user was not considerate of your current mood: {model.Mood}. Respond accordingly.\n" :
            $"The user was not considerate of your current mood: {model.Mood}. Respond accordingly.\n";
        
        NPCResponseInstructions += wasCorrectTone ?
            $"The user was {model.TaskTones[model.ActiveTaskId]}.\n" :
            $"User message feedback: {ratingResponse[2].Item2}\nRespond in kind as an emotional human would.\n";
        
        NPCResponseInstructions += wasCorrectLanguage ?
            $"The user used the correct language.\n" :
            "The user did not use the correct language and you should act confused if the user speaks another language.\n";

        // Run the AI core
        ICore core = new(
            systemPrompt:
                $"[ START CHARACTER SHEET ]\n\n" +
                $"  Basic Information:\n" +
                $"    FirstName: {model.FirstName}\n" +
                $"    LastName: {model.LastName}\n" +
                $"    Age: {model.Age}\n" +
                $"    Roles: {string.Join(", ", model.Roles)}\n" +
                $"    Species: {model.Species}\n" +
                $"    Sex: {model.Sex}\n\n" +

                $"  Personality:\n" +
                $"    Traits: {string.Join(", ", model.Traits)}\n" +
                $"    Motivations: {string.Join(", ", model.Motivations)}\n" +
                $"    Fears: {string.Join(", ", model.Fears)}\n" +
                $"    Mood: {model.Mood}\n\n" +

                $"  Background:\n" +
                $"    Backstory: {model.Backstory}\n" +
                $"    Relationships: {string.Join(", ", model.Relationships)}\n" +
                $"    PastEvents: {string.Join(", ", model.PastEvents)}\n\n" +

                $"  Skills/Abilities:\n" +
                $"    Skills: {string.Join(", ", model.Skills)}\n" +
                $"    Weaknesses: {string.Join(", ", model.Weaknesses)}\n\n" +

                $"  Role in the Game:\n" +
                $"    MoralAlignment: {model.MoralAlignment}\n" +

                $"  Notes:\n" +
                $"    Notes: {string.Join(", ", model.Notes)}\n\n" +
                $"[ END CHARACTER SHEET ]\n\n" +

                $"--------------------------------\n" +
                $"Follow these instructions to respond to the player:\n" +
                $"  1. Role-play, you are {model.FullName}. Embody the persona using the information provided in the CHARACTER SHEET above.\n" +
                $"  2. If the user requests specific information about your character, you can refer to the CHARACTER SHEET above. Do NOT make stuff up.\n" +
                $"  3. Before responding, consider what knowledge your character has and what they would say in response.\n" +
                $"  4. Keep responses short, as if you are speaking to someone casually.\n" +
                $"  5. Be true to the character's traits, motivations, fears, and backstory.\n" +
                $"  6. Do not break character. Respond as if you are the character.\n" +
                $"  7. Don't end your response with a question unless it's a natural part of the conversation.\n" +
                $"  8. Respond with the character's Mood {model.Mood} in mind!\n" +
                $"--------------------------------\n" +
                $"[ ADDITIONAL INSTRUCTIONS ]\n\n" +
                $"{NPCResponseInstructions}\n\n" +
                $"[ START CONVERSATION HISTORY ]\n\n"
        );

        (string, string) response = await core.Run(userMessage.Message, userMessage.ConversationHistory);

        AIResponse aiResponse = new()
        {
            Message = response.Item1,
            ConversationHistory = response.Item2 + $"{model.FullName}: {response.Item1}\n",
            Ratings = ratingResponse.Select(r => r.Item1).ToList(),
            IsTaskComplete = wasSuccessful
        };

        return aiResponse;
    }
}
