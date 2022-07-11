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
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"'Hello {Context.User.Username}#{Context.User.DiscriminatorValue}. Nice to meet you!' in the {Context.Channel.Name} channel.");
    }
}
