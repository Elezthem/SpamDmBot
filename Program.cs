using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;

    static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient();
        _commands = new CommandService();

        await RegisterCommandsAsync();

        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, "YOUR_BOT_TOKEN");
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    public async Task RegisterCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;

        await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
    }

    private Task Log(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(_client, message);

        if (message.Author.IsBot) return;

        int argPos = 0;
        if (message.HasStringPrefix(".", ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
        }
    }

    // Пример команды для рассылки в личные сообщения
    [Command("senddm")]
    public async Task SendDirectMessage([Remainder] string message)
    {
        var users = _client.Guilds.SelectMany(g => g.Users);

        foreach (var user in users)
        {
            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to {user.Username}: {ex.Message}");
            }
        }
    }
}
