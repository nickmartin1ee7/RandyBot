using RandyBot;

using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.AddDiscordService(sp =>
{
    var settings = sp
        .GetRequiredService<IConfiguration>()
        .GetSection(nameof(ClientSettings))
        .Get<ClientSettings>();

    ArgumentNullException.ThrowIfNull(settings, nameof(settings));

    return settings?.DiscordToken
        ?? throw new ArgumentNullException(nameof(ClientSettings.DiscordToken));
});

// Discord
builder.ConfigureServices(serviceCollection =>
{
    serviceCollection.AddSingleton<ClientSettings>(sp =>
    {
        return sp.GetRequiredService<IConfiguration>()
            .GetSection(nameof(ClientSettings))
            .Get<ClientSettings>();
    });

    var settings = serviceCollection
        .BuildServiceProvider()
        .GetRequiredService<ClientSettings>();

    ArgumentNullException.ThrowIfNull(settings, nameof(settings));

    serviceCollection.AddOpenAi(aiSsettings =>
    {
        aiSsettings.ApiKey = settings?.OpenAiToken
             ?? throw new ArgumentNullException(nameof(ClientSettings.OpenAiToken));

        aiSsettings.DefaultRequestConfiguration.Assistant = client =>
        {
            client.WithModel(settings.OpenAiModel
                ?? throw new ArgumentNullException(nameof(ClientSettings.OpenAiModel)));
        };
    }, settings.OpenAiAssistent);

    serviceCollection.AddTransient<ChatCommandSettings>(sp =>
        sp.GetRequiredService<IConfiguration>()
            .GetSection(nameof(ChatCommandSettings))
            .Get<ChatCommandSettings>()
                ?? throw new ArgumentNullException(nameof(ChatCommandSettings)));

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
