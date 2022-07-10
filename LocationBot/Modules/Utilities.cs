using Discord;
using Discord.Commands;
using LocationBot.Common;
using RunMode = Discord.Commands.RunMode;

namespace LocationBot.Modules;

public class Utilities : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("hc", RunMode = RunMode.Async)]
    public async Task HealthCheck()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] MessageAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        await Context.Message.ReplyAsync($"Executing infrastructure health check... please wait.");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"'Executing infrastructure health check... please wait.' in the {Context.Channel.Name} channel.");
    }
}
