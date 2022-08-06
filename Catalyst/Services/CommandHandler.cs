using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Catalyst.Common;
using Catalyst.Init;

namespace Catalyst.Services;

public class CommandHandler : ICommandHandler
{
    private readonly DiscordShardedClient _client;
    private readonly CommandService _commands;

    public CommandHandler(
        DiscordShardedClient client, 
        CommandService commands)
    {
        _client = client;
        _commands = commands;
    }

    public async Task InitializeAsync()
    {
        // add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Bootstrapper.ServiceProvider);
        
        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleCommandAsync;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.ButtonExecuted += ButtonHandler;
        
        _commands.CommandExecuted += async (optional, context, result) =>
        {
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                // the command failed, let's notify the user that something happened.
                await context.Channel.SendMessageAsync($"error: {result}");
            }
        };
        
        foreach (var module in _commands.Modules)
        {
            await Logger.Log(LogSeverity.Info, $"{nameof(CommandHandler)} | Commands", $"Module '{module.Name}' initialized.");
        }
    }
    
    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        if (arg is not SocketUserMessage msg) 
            return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) 
            return;

        // Create a Command Context.
        var context = new ShardedCommandContext(_client, msg);
        
        var markPos = 0;
        if (msg.HasCharPrefix('.', ref markPos) || msg.HasCharPrefix('?', ref markPos))
        {
            var result = await _commands.ExecuteAsync(context, markPos, Bootstrapper.ServiceProvider);
        }
    }

    public async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if (command.Data.Name == "about")
        {
            var whiteCheckMark = new Emoji("\u2705");

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.Data.Name} from the {command.Channel.Name} channel.");
            
            string osEmote = Environment.Is64BitOperatingSystem ? ":white_check_mark:" : ":x:";
            string procEmote = Environment.Is64BitProcess ? ":white_check_mark:" : ":x:";
            string operatingSystem = Environment.OSVersion.ToString().Contains("Microsoft Windows") ? "Microsoft Windows" : Environment.OSVersion.ToString();
#if DEBUG
            string description = $":warning: `THIS IS A PRE-RELEASE VERSION.` :warning:\n\n" +
                $"`Catalyst Version:`  Alpha v0.1 (Build 2207)\n\n" +
                $"__*System Information*__\n" +
                $"`Active Node:`  {Environment.MachineName}\n" +
                $"`Operating System Platform:`  {operatingSystem}\n" +
                $"`Operating System Version:`  {Environment.OSVersion.Version}\n" +
                $"`64 Bit Operating System:`  {osEmote}\n" +
                $"`64 Bit Process:`  {procEmote}\n" +
                $"`.NET Version:`  {Environment.Version}\n\n" +
                $"__*Created By:*__\n" +
                $"> Catalyst#7894\n" +
                $"> Tactical050#9264\n" +
                $"> jxckthxripper#1389\n" +
                $"> 1xs#0001\n" +
                $"> lovelxrd#7895\n\n" +
                $"__*Loaded Modules:*__\n" +
                $"> Utilities Module - v0.1 (Build 2207)\n\n";
#endif
#if RELEASE
            string description = $"`Catalyst Version:`  Alpha v0.1 (Build 2207)\n\n" +
                $"__*System Information*__\n" +
                $"`Active Node:`  {Environment.MachineName}\n" +
                $"`Operating System Platform:`  {operatingSystem}\n" +
                $"`Operating System Version:`  {Environment.OSVersion.Version}\n" +
                $"`64 Bit Operating System:`  {osEmote}\n" +
                $"`64 Bit Process:`  {procEmote}\n" +
                $"`.NET Version:`  {Environment.Version}\n\n" +
                $"__*Created By:*__\n" +
                $"> Catalyst#7894\n" +
                $"> Tactical050#9264\n" +
                $"> jxckthxripper#1389\n" +
                $"> 1xs#0001\n" +
                $"> lovelxrd#7895\n\n" +
                $"__*Loaded Modules:*__\n" +
                $"> Utilities Module - v0.1 (Build 2207)\n\n";
#endif

            var embedded = new EmbedBuilder
            {
                Title = "Catalyst Version Information",
                Description = description,
                Color = new Color(0xF6CF57),
                ImageUrl = "https://raw.githubusercontent.com/CodingCatalysts/Catalyst/main/Catalyst/Assets/Animated%20Logo/Bot_catalyst.gif",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Requested by {command.User.Username}#{command.User.DiscriminatorValue}",
                    IconUrl = command.User.GetAvatarUrl()
                },
                Timestamp = DateTime.Now,
                Author = new EmbedAuthorBuilder
                {
                    Name = "The Catalyst",
                    IconUrl = "https://raw.githubusercontent.com/CodingCatalysts/Catalyst/main/Catalyst/Assets/Animated%20Logo/Bot_catalyst.gif"
                }
            };

            await command.RespondAsync(embed: embedded.Build());

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Application Version information sent to the {command.Channel.Name} channel.");
        }

        if (command.Data.Name == "release_notes")
        {
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.Data.Name} from the {command.Channel.Name} channel.");
            
            await command.RespondAsync(":x: ***NOT IMPLEMENTED*** :x:\n" +
                "This command is under active development and is not yet available.");

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Release Notes sent to the {command.Channel.Name} channel.");
        }

        if (command.Data.Name == "help")
        {
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.Data.Name} from the {command.Channel.Name} channel.");

            await command.RespondAsync(":x: ***NOT IMPLEMENTED*** :x:\n" +
                "This command is under active development and is not yet available.");
            
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Help sent to the {command.Channel.Name} channel.");
        }

        if (command.Data.Name == "temperature")
        {
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.Data.Name} from the {command.Channel.Name} channel.");

            string? unit = command.Data.Options.Last().Value.ToString();
            double temp = double.Parse(command.Data.Options.First().Value.ToString());
            string input = $"`{temp} {unit}:`  ";
            if (unit == "C")
            {
                temp = temp * 9 / 5 + 32;
                unit = "F";
            }
            else
            {
                temp = (temp - 32) * 5 / 9;
                unit = "C";
            }

            await command.RespondAsync($"{input} {temp:0.0} {unit}");
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Temperature Conversion sent to the {command.Channel.Name} channel.  [{input} {temp:0.0} {unit}]");
        }

        if (command.Data.Name == "distance")
        {
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.Data.Name} from the {command.Channel.Name} channel.");

            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();
            double distance = double.Parse(command.Data.Options.ElementAt(0).Value.ToString());
            string input = $"`{distance} {sourceUnit}:`  ";

            if (sourceUnit == "m")
            {
                if (destinationUnit == "m")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "km")
                {
                    distance = distance / 1000;
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance / 1609.34;
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance * 3.28084;
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance * 1.09361;
                }
                else if (destinationUnit == "in")
                {
                    distance = distance * 39.37008;
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 100;
                }
            }
            else if (sourceUnit == "km")
            {
                if (destinationUnit == "m")
                {
                    distance = distance * 1000;
                }
                else if (destinationUnit == "km")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance * 0.621371;
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance * 3280.84;
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance * 1093.61;
                }
                else if (destinationUnit == "in")
                {
                    distance = distance * 39370.08;
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 100000;
                }
            }
            else if (sourceUnit == "mi")
            {
                if (destinationUnit == "m")
                {
                    distance = distance * 1609.34;
                }
                else if (destinationUnit == "km")
                {
                    distance = distance * 1.60934;
                }
                else if (destinationUnit == "mi")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance * 5280;
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance * 1760;
                }
                else if (destinationUnit == "in")
                {
                    distance = distance * 63360;
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 1609340;
                }
            }
            else if (sourceUnit == "ft")
            {
                if (destinationUnit == "m")
                {
                    distance = distance / 3.28084;
                }
                else if (destinationUnit == "km")
                {
                    distance = distance / 3280.84;
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance / 5280;
                }
                else if (destinationUnit == "ft")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance / 3;
                }
                else if (destinationUnit == "in")
                {
                    distance = distance * 12;
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 30.48;
                }
            }
            else if (sourceUnit == "yd")
            {
                if (destinationUnit == "m")
                {
                    distance = distance / 1.09361;
                }
                else if (destinationUnit == "km")
                {
                    distance = distance / 1093.61;
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance / 1760;
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance * 3;
                }
                else if (destinationUnit == "yd")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "in")
                {
                    distance = distance * 36;
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 91.44;
                }
            }
            else if (sourceUnit == "in")
            {
                if (destinationUnit == "m")
                {
                    distance = distance / 39.37008;
                }
                else if (destinationUnit == "km")
                {
                    distance = distance / 39370.08;
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance / 63360;
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance / 12;
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance / 36;
                }
                else if (destinationUnit == "in")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = distance * 2.54;
                }
            }
            else if (sourceUnit == "cm")
            {
                if (destinationUnit == "m")
                {
                    distance = distance / 100;
                }
                else if (destinationUnit == "km")
                {
                    distance = distance / 100000;
                }
                else if (destinationUnit == "mi")
                {
                    distance = distance / 1609340;
                }
                else if (destinationUnit == "ft")
                {
                    distance = distance / 30.48;
                }
                else if (destinationUnit == "yd")
                {
                    distance = distance / 91.44;
                }
                else if (destinationUnit == "in")
                {
                    distance = distance / 2.54;
                }
                else if (destinationUnit == "cm")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
            }

            await command.RespondAsync($"{input} {distance:0.0} {destinationUnit}");
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Distance Conversion sent to the {command.Channel.Name} channel.  [{input} {distance:0.0} {destinationUnit}]");
        }
    }
    
    public async Task ButtonHandler(SocketMessageComponent component)
    {
        await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] CommandReceived", $"{component.User.Username}#{component.User.DiscriminatorValue} has invoked {component.Data.CustomId} from the {component.Channel.Name} channel.");
        var embed = new EmbedBuilder
        {
            Title = "Wick Command Reference Guide",
            Color = new Color(0x00f7ff),
            Footer = new EmbedFooterBuilder
            {
                Text = $"Requested by {component.User.Username}#{component.User.DiscriminatorValue}",
                IconUrl = component.User.GetAvatarUrl()
            },
            Timestamp = DateTime.Now,
            Author = new EmbedAuthorBuilder
            {
                Name = "The Catalyst",
                IconUrl = "https://raw.githubusercontent.com/CodingCatalysts/Catalyst/main/Catalyst/Assets/Animated%20Logo/Bot_catalyst.gif"
            },
        };

        // We can now check for our custom id
        switch (component.Data.CustomId)
        {
            // Since we set our buttons custom id as 'custom-id', we can check for it like this:
            case "overview":
                embed.Description = $"The server currently uses `Wick Bot` for moderation.\n" +
                "This Guide will describe the commands that will be needed during an incident.\n\n" +
                "> `PLEASE NOTE:` for commands that have multiple options\n" +
                "> (ex. @User#0001 or UserID) `or` will be designated by `|`.\n" +
                "> \n" +
                "> `OPTIONAL INPUTS:` are denoted in braces { ex. } and are **not** required.\n" +
                "> Not including these inputs may have consequences.\n\n" +
                "See docs included with each command for details.\n" +
                "`Please click one of the buttons for command details.`\n\n" +
                "`Done:`  Ends interaction, keeping this message open.\n" +
                "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "mute":
                // Lets respond by sending a message saying they clicked the button
                embed.Title += " - Mute";
                embed.Description = $"Muting a user prevents them from sending messages or connecting to voice.\n" +
                    $"A DM will be sent to the user(s) warned informing them of the action.\n\n" +
                    ":warning:  `Time is optional.`  **Not including a time will result in a Perma Mute!!!**  :warning:\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+mute @User | UserID ?r You have been muted.  <Reason>.  Please review the Server Rules.  Repeat offenses will result in a longer duration or additional action. {?t #(m/h/d)}\n\n" +
                    "+mute @1xs#0001 ?r You have been muted.  Come at me Server Owner.  Please review the Server Rules.  Repeat offenses will result in a longer duration or additional action. ?t 1h\n\n" +
                    "+mute @1xs#0001, @Catalyst#7894 ?r You have been muted.  Come at me Mr. Server Owner. Please review the Server Rules.  Repeat offenses will result in a longer duration or additional action. ?t 1h\n\n" +
                    "+mute 587220709382684673 ?r You have been muted.  Come at me Server Owner. Please review the Server Rules.  Repeat offenses will result in a longer duration or additional action.\n\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Done:`  Ends interaction, keeping this message open.\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "warn":
                // Lets respond by sending a message saying they clicked the button
                embed.Title += " - Warnings";
                embed.Description = $"Issue a warning to the user for a violation.  Too many warnings, action will be taken.\n" +
                    $"A DM will be sent to the user(s) warned informing them of the action.\n\n" +
                    $"```\n" +
                    $"Command Syntax:\n" +
                    $"+warn @User | UserID ?r <Reason>\n\n" +
                    $"+warn Ascended#1023 ?r Didn't even realize who Catalyst#7894 was on Instagram.\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Done:`  Ends interaction, keeping this message open.\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "kick":
                // Lets respond by sending a message saying they clicked the button
                embed.Title += " - Kick";
                embed.Description = $"User will be immediately kicked from the server.\n" +
                    $"A DM will be sent to the user(s) being kicked informing them of the action.\n\n" +
                    $":warning:  A kicked user will be able to immediately rejoin the server.  :warning:\n" +
                    $"```\n" +
                    $"Command Syntax:\n" +
                    $"+kick @User | UserID ?r <Reason>\n\n" +
                    $"+kick @1xs#0001 ?r There is no way this command would ever work.\n\n" +
                    $"+kick @Catalyst#7894, #Ascended#1023 ?r They lost GHXST's fit battle.\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Done:`  Ends interaction, keeping this message open.\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "ban":
                // Lets respond by sending a message saying they clicked the button
                embed.Title += " - Bans";
                embed.Description = "User will be immediately banned from the server.\n" +
                    "A DM will be sent to the user(s) being banned informing them of the action.\n\n" +
                    ":warning:  `Time is optional.`  **Not including a time will result in a Perma Ban!!!**  :warning:\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+ban @User | UserID ?r <Reason> {?t #(m/h/d)}\n\n" +
                    "+ban 1xs#0001 ?r How dare you actually take a vacation. ?t 14d\n\n" +
                    "+ban 1xs#0001, Catalyst#7894 ?r Who needs IT Professionals anyway." +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Done:`  Ends interaction, keeping this message open.\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "purge":
                // Lets respond by sending a message saying they clicked the button
                embed.Title += " - Purge";
                embed.Description = "Deletes number of specified recent messages within the channel executed.\n\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+purge #\n\n" +
                    "+purge 10\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Done:`  Ends interaction, keeping this message open.\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} sent to the {component.Channel.Name} channel.");
                break;

            case "close":
                // Lets respond by sending a message saying they clicked the button
                var message = component.Message;
                await message.DeleteAsync();
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ResponseSent", $"{component.Data.CustomId} executed from the {component.Channel.Name} channel.");
                break;
        }
    }
}
