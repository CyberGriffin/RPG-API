namespace API5.Models;

public class CharacterModel
{
    // Basic Information
    public string FirstName = "";
    public string LastName = "";
    public string FullName => FirstName + " " + LastName;
    public int Age = 0;
    public List<string> Roles = new(); // NPC's role in the game world
    public string Species = ""; // Species/race of the NPC
    public string Sex = "";

    // Personality
    public List<string> Traits = new();
    public List<string> Motivations = new();
    public List<string> Fears = new();
    public string Mood = ""; // Dictates the emotion behind the NPC's responses

    // Background
    public string Backstory = "";
    public List<string> Relationships = new();
    public List<string> PastEvents = new();

    // Skills/Abilities
    public List<string> Skills = new();
    public List<string> Weaknesses = new();

    // Role in the Game
    public Dictionary<int, string> TaskInteractions = new();
    public Dictionary<int, string> TaskTones = new();
    public Dictionary<int, List<int>> TaskSuccessConditions = new();
    public int ActiveTaskId = -1;
    public string MoralAlignment = "";
    public List<string> PlotPoints = new();

    // Notes
    public List<string> Notes = new();

    public override string ToString()
    {
        string output = "";

        output += $"[ START CHARACTER SHEET ]\n\n";
        output += $"  Basic Information:\n";
        output += $"    FirstName: {FirstName}\n";
        output += $"    LastName: {LastName}\n";
        output += $"    Age: {Age}\n";
        output += $"    Roles: {string.Join(", ", Roles)}\n";
        output += $"    Species: {Species}\n";
        output += $"    Sex: {Sex}\n\n";

        output += $"  Personality:\n";
        output += $"    Traits: {string.Join(", ", Traits)}\n";
        output += $"    Motivations: {string.Join(", ", Motivations)}\n";
        output += $"    Fears: {string.Join(", ", Fears)}\n";
        output += $"    Mood: {Mood}\n\n";

        output += $"  Background:\n";
        output += $"    Backstory: {Backstory}\n";
        output += $"    Relationships: {string.Join(", ", Relationships)}\n";
        output += $"    PastEvents: {string.Join(", ", PastEvents)}\n\n";

        output += $"  Skills/Abilities:\n";
        output += $"    Skills: {string.Join(", ", Skills)}\n";
        output += $"    Weaknesses: {string.Join(", ", Weaknesses)}\n\n";

        output += $"  Role in the Game:\n";
        output += $"    TaskInteractions: {string.Join(", ", TaskInteractions)}\n";
        output += $"    TaskTones: {string.Join(", ", TaskTones)}\n";
        output += $"    TaskSuccessConditions: {string.Join(", ", TaskSuccessConditions)}\n";
        output += $"    ActiveTaskId: {ActiveTaskId}\n";
        output += $"    PlotPoints: {string.Join(", ", PlotPoints)}\n";
        output += $"    MoralAlignment: {MoralAlignment}\n\n";

        output += $"  Notes:\n";
        output += $"    {string.Join("\n    ", Notes)}\n\n";

        return output;
    }
}
