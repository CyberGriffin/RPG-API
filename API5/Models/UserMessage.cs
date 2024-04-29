using API5.AI;

namespace API5.Models;

public class UserMessage
{
    public string Message { get; set; } = "";
    private string? FilteredMessage { get; set; } = null;
    public string ConversationHistory { get; set; } = "";
    public string CharacterJson { get; set; } = "";

    public async Task<string> GetFilteredMessage()
    {
        FilteredMessage ??= await ICoreUtils.Filter(Message);
        return FilteredMessage;
    }
}

// public class CharacterModel
// {
//     // Basic Information
//     public string FirstName = "";
//     public string LastName = "";
//     public string FullName => FirstName + " " + LastName;
//     public int Age = 0;
//     public List<string> Roles = new(); // NPC's role in the game world
//     public string Species = ""; // Species/race of the NPC
//     public string Sex = "";

//     // Personality
//     public List<string> Traits = new();
//     public List<string> Motivations = new();
//     public List<string> Fears = new();
//     public string Mood = ""; // Dictates the emotion behind the NPC's responses

//     // Background
//     public string Backstory = "";
//     public List<string> Relationships = new();
//     public List<string> PastEvents = new();

//     // Skills/Abilities
//     public List<string> Skills = new();
//     public List<string> Weaknesses = new();

//     // Role in the Game
//     public Dictionary<int, string> TaskInteractions = new();
//     public Dictionary<int, string> TaskTones = new();
//     public Dictionary<int, List<int>> TaskSuccessConditions = new();
//     public int ActiveTaskId = -1;
//     public string MoralAlignment = "";
//     public List<string> PlotPoints = new();

//     // Notes
//     public List<string> Notes = new();