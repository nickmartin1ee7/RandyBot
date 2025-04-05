using System.ComponentModel;
using System.Net.Http.Json;

using RandyBot;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

using Rystem.OpenAi;

public class UserCommands : CommandGroup
{
    private readonly ILogger<UserCommands> _logger;
    private readonly IOpenAi _aiClient;
    private readonly IFeedbackService _feedbackService;
    private readonly ClientSettings _clientSettings;
    private readonly ChatCommandSettings _chatCommandSettings;

    public UserCommands(
        ILogger<UserCommands> logger,
        IOpenAi aiClient,
        IFeedbackService feedbackService,
        ClientSettings clientSettings,
        ChatCommandSettings chatCommandSettings)
    {
        _logger = logger;
        _aiClient = aiClient;
        _feedbackService = feedbackService;
        _clientSettings = clientSettings;
        _chatCommandSettings = chatCommandSettings;
    }

    [Command("chat", "randy", "savage")]
    [Description("OH YEAH, DIG IT, BROTHER!")]
    [CommandType(ApplicationCommandType.ChatInput)]
    public async ValueTask<Result> ChatCommand([Description("Tell me what's cookin', Brother!")] string message)
    {
        try
        {
            var assistants = await _aiClient.Assistant.ListAsync(cancellationToken: CancellationToken);
            var aiRequest = assistants.Data!.First(x => x.Id == _clientSettings.OpenAiAssistent);
            var ai = await _aiClient.Assistant.RetrieveAsync(aiRequest.Id!, CancellationToken);
            var msg = _aiClient.Chat
                .AddAssistantMessage(_chatCommandSettings.SystemPrompt)
                .AddUserMessage(message)
                ;
            var response = await msg.ExecuteAsync(CancellationToken);

            var responseMsg = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(responseMsg))
            {
                throw new ArgumentException("Chat Response contained no context!", nameof(responseMsg));
            }

            await _feedbackService.SendContextualSuccessAsync(responseMsg);

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to finish chat command!");

            var errorReply = await _feedbackService.SendContextualErrorAsync(_chatCommandSettings.ErrorResponse!);

            return Result.FromError(errorReply);
        }
    }
}