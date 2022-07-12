using Discord;
using Discord.Commands;
using Catalyst.Common;
using RunMode = Discord.Commands.RunMode;

namespace Catalyst.Modules;

public class Core : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("about", RunMode = RunMode.Async)]
    public async Task AboutBot()
    {
        var whiteCheckMark = new Emoji("\u2705");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        await Context.Channel.TriggerTypingAsync();

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
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :x: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
    }

    [Command("help", RunMode = RunMode.Async)]
    public async Task Help()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var redX = new Emoji("\u274C");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(redX);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :x: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");
    }

    [Command("sc", RunMode = RunMode.Async)]
    public async Task StaffCommands()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var redX = new Emoji("\u274C");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        await Context.Channel.TriggerTypingAsync();

        string message = @"@STAFF 
**COMMAND QUICK REFERENCE GUIDE**
The server currently uses multiple different bots for different actions which can lead to confusion when action needs to be taken.
This Quick Reference Guide will describe the critical commands that will be needed during an incident.
> `PLEASE NOTE:` for commands that have multiple options (ex. @User#0001 or UserID) `or` will be designated by `|`.
> 
> `OPTIONAL COMMANDS:` will be denoted in braces { ex. }... optional commands are NOT required, but not including them may have additional consequences.  See docs for each command below for details.";

await Context.Channel.SendMessageAsync(message);
await Context.Channel.TriggerTypingAsync();

message = @"**User Actions:**
`Mute:`  Muting a user prevents them from sending messages or connecting to voice. A DM will be sent to the user(s) warned informing them of the action.
:warning:  Time is optional.  **Not including a time will result in a Perma Mute!!!**  :warning:
```
Command Syntax:
+mute @User | UserID ?r You have been muted.  <Reason>  Please review the rules in ┃welcome.  Repeat offenses will result in a longer duration or additional action. {?t #(m/h/d)}

+mute @1xs#0001 ?r You have been muted.  Come at me Mr. Server Owner. Please review the rules in ┃welcome.  Repeat offenses will result in a longer duration or additional action. ?t 1h

+mute @1xs#0001, @Catalyst#7894 ?r You have been muted.  Come at me Mr. Server Owner. Please review the rules in ┃welcome.  Repeat offenses will result in a longer duration or additional action. ?t 1h

+mute 587220709382684673 ?r You have been muted.  Come at me Mr. Server Owner. Please review the rules in ┃welcome.  Repeat offenses will result in a longer duration or additional action.
```
`Warn:`  Issue a warning to the user for a violation.  A DM will be sent to the user(s) warned informing them of the action.
```
Command Syntax:
+warn @User | UserID ?r <Reason>
+warn Ascended#1023 ?r Didn't even realize who Catalyst#7894 was on Instagram.
```
`Kick:`  User will be immediately kicked from the server.  A DM will be sent to the user(s) being kicked informing them of the action.
:warning:  A kicked user will be able to immediately rejoin the server.  :warning:
```
Command Syntax:
+kick @User | UserID ?r <reason>
+kick @1xs#0001 ?r There is no way this command would ever work.
+kick @Catalyst#7894, #Ascended#1023 ?r They lost GHXST's fit battle.
```";

await Context.Channel.SendMessageAsync(message);
await Context.Channel.TriggerTypingAsync();

message = @"`Ban:`  User will be immediately banned from the server.  A DM will be sent to the user(s) being banned informing them of the action.
```
Command Syntax:
+ban @User | UserID ?r <Reason>
+ban 1xs#0001 ?r How dare you actually take a vacation. <3
+ban 1xs#0001, Catalyst#7894 ?r Who needs IT Professionals anyway.
```";

await Context.Channel.SendMessageAsync(message);
await Context.Channel.TriggerTypingAsync();

message = @"**Giveaway Commands**
`gcreate:`  Creates a giveaway in the giveaway channel.
:warning:  This command has no command syntax.  It will walk you through the process of setting up a giveaway.  :warning:
```
Command Syntax:
/gcreate
```";

        await Context.Channel.SendMessageAsync(message);
    }
}
