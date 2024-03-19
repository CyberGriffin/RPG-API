using Microsoft.AspNetCore.Mvc;

namespace API4.Models;

public class OrchestratorModel
{
    public string? Task { get; set; } = "";
    public string[]? ConversationTopics { get; set; } = Array.Empty<string>();
    public string PlayerPrompt { get; set; } = "";

    public IActionResult IsValid()
    {
        if (string.IsNullOrEmpty(Task?.Trim()) && (ConversationTopics == null || ConversationTopics.Length == 0))
        {
            return new BadRequestObjectResult("Tasks and conversation topics were not provided.");
        }

        if (string.IsNullOrEmpty(PlayerPrompt.Trim()))
        {
            return new BadRequestObjectResult("Player prompt was null or empty.");
        }

        return new OkResult();
    }

    public bool HasTask()
    {
        return !string.IsNullOrEmpty(Task);
    }

    public bool HasConversationTopics()
    {
        return ConversationTopics != null && ConversationTopics.Length > 0;
    }
}
