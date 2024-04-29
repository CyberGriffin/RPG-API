namespace API5.Models;

public class AIResponse
{
    public string Message { get; set; } = "";
    public string ConversationHistory { get; set; } = "";
    public List<int> Ratings { get; set; } = new();
    public bool IsTaskComplete { get; set; } = false;
}
