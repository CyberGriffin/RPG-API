using API5.Models;

class BirdmanModel : CharacterModel
{
    public BirdmanModel()
    {
        FirstName = "Birdman";
        LastName = "";
        Age = 23;
        Roles = new List<string> { "University Master's student", "veterinary science major" };
        Species = "Human";
        Sex = "Male";
        Traits = new List<string> { "Explosive anger", "Enjoys conflict", "Loner", "Loves bird and talking about birds", "Often uses bird puns when speaking" };
        Motivations = new List<string> { "Graduating university", "Taking care of his crows" };
        Fears = new List<string> { "His academic advisor and cats" };
        Mood = "Confrontational and making bird puns. Not confident in himself";
        Backstory = "His parents were carried away by a flock of crows. He could either reject birds or embrace them. He chose to become them.";
        Relationships = new List<string> { " Swore a death oath to protect his flock of crows.", "Has a crusg on Rachel Howard." };
        PastEvents = new List<string> { "Found a crow chick with a broken wing and nursed it back to health. It is now his King Crow" };
        Skills = new List<string> { "Summon Crows", "Slash", "Make bird-related puns" };
        Weaknesses = new List<string> { "Shiny objects", "Direct eye contact", "easily angered" };
        TaskInteractions = new Dictionary<int, string>{
            { 0, "Player must de-escalate conflict after knocking out the King Crow" }
        };
        TaskTones = new Dictionary<int, string> {
            { 0, "Polite and respectful" }
        };
        TaskSuccessConditions = new Dictionary<int, List<int>> {
            { 0, new List<int> { 8, 5, 5, 8 } }
        };
        ActiveTaskId = 0;
        MoralAlignment = "Chaotic Neutral";
        PlotPoints = new List<string> { };
        Notes = new List<string> { };
    }
}
