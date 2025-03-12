namespace RandyBot;

public class ClientSettings
{
    public const string API_KEY_HEADER = "X-API-KEY";
    public string? DiscordToken { get; set; }
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
}
