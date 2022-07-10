using Discord;
using Discord.Commands;
using LocationBot.Common;
using RunMode = Discord.Commands.RunMode;

namespace LocationBot.Modules;

public class Core : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("about", RunMode = RunMode.Async)]
    public async Task AboutBot()
    {
        var whiteCheckMark = new Emoji("\u2705");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] MessageAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
        
        await Context.Message.ReplyAsync($"\n__**About [PLACEHOLDER]**__\n" +
            $":warning: `THIS IS A PRE-RELEASE VERSION.` :warning:\n\n" +
            $"`[PLACEHOLDER] Version:`  Alpha Development Release v0.1 (Build 2207)\n\n" +
            $"__*System Information*__\n" +
            $"`Active Node:`  {Environment.MachineName}\n" +
            $"`Operating System Version:`  {Environment.OSVersion}\n" +
            $"`64 Bit Operating System:`  {Environment.Is64BitOperatingSystem}\n" +
            $"`64 Bit Process:`  {Environment.Is64BitProcess}\n" +
            $"`.NET Version:`  {Environment.Version}\n\n" +
            $"__*Created By:*__\n" +
            $"> Catalyst#7894\n" +
            $"> 1xs#0001\n" +
            $"> lovelxrd#7895\n\n" +
            $"__*Loaded Modules:*__\n" +
            $"> Core Command Module - v0.1 (Build 2207)\n" +
            $"> Utilities Module - v0.1 (Build 2207)\n\n" +
            $"__*Documentation*__\n" +
            $"> Are you kidding... [PLACEHOLDER]\n" +
            $"> The Change Log can be accessed by executing `!changelog`");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"'Application Version information sent to the {Context.Channel.Name} channel.");
    }
    
    [Command("changelog", RunMode = RunMode.Async)]
    public async Task ChangeLog()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var noEntry = new Emoji("\u274C");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(noEntry);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] MessageAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
    }
}
