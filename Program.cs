using RandyBot;

using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.AddDiscordService(sp =>
    sp.GetRequiredService<IConfiguration>()
        .GetSection(nameof(ClientSettings))
        .Get<ClientSettings>()?.DiscordToken
        ?? throw new ArgumentNullException(nameof(ClientSettings.DiscordToken)));

// Discord
builder.ConfigureServices(serviceCollection =>
{
    serviceCollection.AddSingleton<ChatCommandSettings>(sp =>
        sp.GetRequiredService<IConfiguration>()
             .GetSection(nameof(ChatCommandSettings))
             .Get<ChatCommandSettings>()
             ?? throw new ArgumentNullException(nameof(ChatCommandSettings)));

    serviceCollection.AddHttpClient<UserCommands>(
        nameof(UserCommands),
        (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            client.BaseAddress = new Uri(config
                .GetSection(nameof(ClientSettings))
                .Get<ClientSettings>()?.ApiUrl
                 ?? throw new ArgumentNullException(nameof(ClientSettings.ApiUrl)));

            client.DefaultRequestHeaders.TryAddWithoutValidation(
                ClientSettings.API_KEY_HEADER,
                config
                    .GetSection(nameof(ClientSettings))
                    .Get<ClientSettings>()?.ApiKey
                    ?? throw new ArgumentNullException(nameof(ClientSettings.ApiKey)));
        });

    serviceCollection
        .AddDiscordCommands(true)
        .AddCommandTree()
        .WithCommandGroup<UserCommands>()
        .Finish();

    var responderTypes = typeof(Program).Assembly
        .GetExportedTypes()
        .Where(t => t.IsResponder());

    foreach (var responderType in responderTypes)
    {
        serviceCollection.AddResponder(responderType);
    }
});
var host = builder.Build();

host.Run();
