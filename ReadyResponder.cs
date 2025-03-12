using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace RandyBot;

public class ReadyResponder : IResponder<IReady>
{
    private readonly ILogger<ReadyResponder> _logger;
    private readonly DiscordGatewayClient _discordGatewayClient;
    private readonly SlashService _slashService;

    public ReadyResponder(ILogger<ReadyResponder> logger, DiscordGatewayClient discordGatewayClient, SlashService slashService)
    {
        _logger = logger;
        _discordGatewayClient = discordGatewayClient;
        _slashService = slashService;
    }

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        UpdatePresence();
        await UpdateGlobalSlashCommands();

        _logger.LogInformation("I'm ready!");

        return Result.FromSuccess();

        void UpdatePresence()
        {
            var updateCommand = new UpdatePresence(UserStatus.Online, false, null, new IActivity[]
            {
                new Activity("for the verbal beatdown!", ActivityType.Watching)
            });

            _discordGatewayClient.SubmitCommand(updateCommand);
        }

        async Task UpdateGlobalSlashCommands()
        {
            var updateResult = await _slashService.UpdateSlashCommandsAsync(ct: ct);

            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Failed to update application commands globally!");
            }
        }
    }
}
