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

        string osEmote = Environment.Is64BitOperatingSystem ? ":white_check_mark:" : ":x:";
        string procEmote = Environment.Is64BitProcess ? ":white_check_mark:" : ":x:";

        await Context.Message.ReplyAsync($"\n__**About [PLACEHOLDER]**__\n" +
            $":warning: `THIS IS A PRE-RELEASE VERSION.` :warning:\n\n" +
            $"`[PLACEHOLDER] Version:`  Alpha Development Release v0.1 (Build 2207)\n\n" +
            $"__*System Information*__\n" +
            $"`Active Node:`  {Environment.MachineName}\n" +
            $"`Operating System Version:`  {Environment.OSVersion}\n" +
            $"`64 Bit Operating System:`  {osEmote}\n" +
            $"`64 Bit Process:`  {procEmote}\n" +
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
            $"> The Change Log can be accessed by executing `!changelog`\n" +
            $"> A list of all commands can be accessed by executing `!help`");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"Application Version information sent to the {Context.Channel.Name} channel.");
    }
    
    [Command("changelog", RunMode = RunMode.Async)]
    public async Task ChangeLog()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var redX = new Emoji("\u274C");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(redX);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] MessageAcknowledged", $"Reacted with :x: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
    }

    [Command("help", RunMode = RunMode.Async)]
    public async Task Help()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var redX = new Emoji("\u274C");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(redX);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] MessageAcknowledged", $"Reacted with :x: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
    }
}
