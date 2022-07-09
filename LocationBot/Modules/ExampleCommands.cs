using Discord;
using Discord.Commands;
using LocationBot.Common;
using RunMode = Discord.Commands.RunMode;

namespace LocationBot.Modules;

public class ExampleCommands : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("hello", RunMode = RunMode.Async)]
    public async Task Hello()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, "CommandReceived", $"{Context.User.Username} has invoked !hello from the {Context.Channel.Name} channel.");
        
        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
        await Logger.Log(LogSeverity.Verbose, "ResponseSent", $"'Hello { Context.User.Username}. Nice to meet you!' in the {Context.Channel.Name} channel.");
    }
}
