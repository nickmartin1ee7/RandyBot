using System.ComponentModel;
using System.Net.Http.Json;

using RandyBot;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

public class UserCommands : CommandGroup
{
    private readonly ILogger<UserCommands> _logger;
    private readonly HttpClient _client;
    private readonly IFeedbackService _feedbackService;
    private readonly ChatCommandSettings _chatCommandSettings;

    public UserCommands(ILogger<UserCommands> logger, HttpClient client, IFeedbackService feedbackService, ChatCommandSettings chatCommandSettings)
    {
        _logger = logger;
        _client = client;
        _feedbackService = feedbackService;
        _chatCommandSettings = chatCommandSettings;
    }

    [Command("chat", "randy", "savage")]
    [Description("OH YEAH, DIG IT, BROTHER!")]
    [CommandType(ApplicationCommandType.ChatInput)]
    public async ValueTask<Result> ChatCommand([Description("Tell me what's cookin', Brother!")] string message)
    {
        try
        {
            var httpResponse = await _client.PostAsync("/api/v2/chat", JsonContent.Create(BuildChatMessage(message)));

            _logger.LogInformation("HTTP Response ({httpResponseReasonPhrase} {httpResponseStatusCode}): {httpResponseContent}",
                httpResponse.ReasonPhrase,
                (int)httpResponse.StatusCode,
                await httpResponse.Content.ReadAsStringAsync());

            var chatResponse = await httpResponse.Content.ReadFromJsonAsync<ChatResponse>()
                ?? throw new Exception($"Failed to get a chat response from client!");

            if (string.IsNullOrWhiteSpace(chatResponse.Message))
            {
                throw new ArgumentException("Chat Response contained no context!", nameof(chatResponse.Message));
            }

            await _feedbackService.SendContextualSuccessAsync(chatResponse.Message);

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to finish chat command!");

            var errorReply = await _feedbackService.SendContextualErrorAsync(_chatCommandSettings.ErrorResponse!);

            return Result.FromError(errorReply);
        }
    }

    private object BuildChatMessage(string message)
    {
        return $"System: {_chatCommandSettings.SystemPrompt}" +
            Environment.NewLine +
            $"User: {message}";
    }
}

internal class ChatResponse
{
    public string? Message { get; set; }
}