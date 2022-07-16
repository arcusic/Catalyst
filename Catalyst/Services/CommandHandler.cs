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
        if (msg.HasCharPrefix('!', ref markPos) || msg.HasCharPrefix('?', ref markPos))
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
                $"> 1xs#0001\n" +
                $"> lovelxrd#7895\n\n" +
                $"__*Loaded Modules:*__\n" +
                $"> Core Command Module - v0.1 (Build 2207)\n" +
                $"> Utilities Module - v0.1 (Build 2207)\n\n" +
                $"__*Documentation*__\n" +
                $"> Change Log can be viewed by `!changelog`\n" +
                $"> Commands can be viewed by executing `!help`";
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
            $"> 1xs#0001\n" +
            $"> lovelxrd#7895\n\n" +
            $"__*Loaded Modules:*__\n" +
            $"> Utilities Module - v0.1 (Build 2207)\n\n" +
#endif

            var embedded = new EmbedBuilder
            {
                Title = "Catalyst Version Information",
                Description = description,
                Color = new Color(0xF6CF57),
                ImageUrl = "https://cdn.discordapp.com/attachments/994640322615324773/997325232215957514/unknown.png",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Requested by {command.User.Username}#{command.User.DiscriminatorValue}",
                    IconUrl = command.User.GetAvatarUrl()
                },
                Timestamp = DateTime.Now,
                Author = new EmbedAuthorBuilder
                {
                    Name = "The Catalyst",
                    IconUrl = "https://cdn.discordapp.com/attachments/994640322615324773/997325232215957514/unknown.png"
                }
            };

            await command.RespondAsync(embed: embedded.Build());

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Application Version information sent to the {command.Channel.Name} channel.");
        }

        if (command.Data.Name == "release_notes")
        {
            await command.RespondAsync(":x: ***NOT IMPLEMENTED*** :x:\n" +
                "This command is under active development and is not yet available.");
        }

        if (command.Data.Name == "help")
        {
            await command.RespondAsync(":x: ***NOT IMPLEMENTED*** :x:\n" +
                "This command is under active development and is not yet available.");
        }

        if (command.Data.Name == "temperature")
        {
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
        }

        if (command.Data.Name == "distance")
        {
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
        }
    }
}
