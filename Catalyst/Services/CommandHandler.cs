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
                    distance /= 1000;
                }
                else if (destinationUnit == "mi")
                {
                    distance /= 1609.34;
                }
                else if (destinationUnit == "ft")
                {
                    distance *= 3.28084;
                }
                else if (destinationUnit == "yd")
                {
                    distance *= 1.09361;
                }
                else if (destinationUnit == "in")
                {
                    distance *= 39.37008;
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 100;
                }
            }
            else if (sourceUnit == "km")
            {
                if (destinationUnit == "m")
                {
                    distance *= 1000;
                }
                else if (destinationUnit == "km")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mi")
                {
                    distance *= 0.621371;
                }
                else if (destinationUnit == "ft")
                {
                    distance *= 3280.84;
                }
                else if (destinationUnit == "yd")
                {
                    distance *= 1093.61;
                }
                else if (destinationUnit == "in")
                {
                    distance *= 39370.08;
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 100000;
                }
            }
            else if (sourceUnit == "mi")
            {
                if (destinationUnit == "m")
                {
                    distance *= 1609.34;
                }
                else if (destinationUnit == "km")
                {
                    distance *= 1.60934;
                }
                else if (destinationUnit == "mi")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "ft")
                {
                    distance *= 5280;
                }
                else if (destinationUnit == "yd")
                {
                    distance *= 1760;
                }
                else if (destinationUnit == "in")
                {
                    distance *= 63360;
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 1609340;
                }
            }
            else if (sourceUnit == "ft")
            {
                if (destinationUnit == "m")
                {
                    distance /= 3.28084;
                }
                else if (destinationUnit == "km")
                {
                    distance /= 3280.84;
                }
                else if (destinationUnit == "mi")
                {
                    distance /= 5280;
                }
                else if (destinationUnit == "ft")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "yd")
                {
                    distance /= 3;
                }
                else if (destinationUnit == "in")
                {
                    distance *= 12;
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 30.48;
                }
            }
            else if (sourceUnit == "yd")
            {
                if (destinationUnit == "m")
                {
                    distance /= 1.09361;
                }
                else if (destinationUnit == "km")
                {
                    distance /= 1093.61;
                }
                else if (destinationUnit == "mi")
                {
                    distance /= 1760;
                }
                else if (destinationUnit == "ft")
                {
                    distance *= 3;
                }
                else if (destinationUnit == "yd")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "in")
                {
                    distance *= 36;
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 91.44;
                }
            }
            else if (sourceUnit == "in")
            {
                if (destinationUnit == "m")
                {
                    distance /= 39.37008;
                }
                else if (destinationUnit == "km")
                {
                    distance /= 39370.08;
                }
                else if (destinationUnit == "mi")
                {
                    distance /= 63360;
                }
                else if (destinationUnit == "ft")
                {
                    distance /= 12;
                }
                else if (destinationUnit == "yd")
                {
                    distance /= 36;
                }
                else if (destinationUnit == "in")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "cm")
                {
                    distance *= 2.54;
                }
            }
            else if (sourceUnit == "cm")
            {
                if (destinationUnit == "m")
                {
                    distance /= 100;
                }
                else if (destinationUnit == "km")
                {
                    distance /= 100000;
                }
                else if (destinationUnit == "mi")
                {
                    distance /= 1609340;
                }
                else if (destinationUnit == "ft")
                {
                    distance /= 30.48;
                }
                else if (destinationUnit == "yd")
                {
                    distance /= 91.44;
                }
                else if (destinationUnit == "in")
                {
                    distance /= 2.54;
                }
                else if (destinationUnit == "cm")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {distance:0.0} {destinationUnit}");
                }
            }

            await command.RespondAsync($"{input} {distance:0.0} {destinationUnit}");
        }

        if (command.Data.Name == "weight")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();
            double weight = double.Parse(command.Data.Options.ElementAt(0).Value.ToString());
            string input = $"`{weight} {sourceUnit}:`  ";

            if (sourceUnit == "kg")
            {
                if (destinationUnit == "kg")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {weight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "g")
                {
                    weight *= 1000;
                }
                else if (destinationUnit == "lb")
                {
                    weight *= 2.20462;
                }
                else if (destinationUnit == "oz")
                {
                    weight *= 35.274;
                }
            }
            else if (sourceUnit == "g")
            {
                if (destinationUnit == "kg")
                {
                    weight /= 1000;
                }
                else if (destinationUnit == "g")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {weight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "lb")
                {
                    weight *= 0.00220462;
                }
                else if (destinationUnit == "oz")
                {
                    weight *= 0.035274;
                }
            }
            else if (sourceUnit == "lb")
            {
                if (destinationUnit == "kg")
                {
                    weight /= 2.20462;
                }
                else if (destinationUnit == "g")
                {
                    weight *= 453.592;
                }
                else if (destinationUnit == "lb")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {weight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "oz")
                {
                    weight *= 16;
                }
            }
            else if (sourceUnit == "oz")
            {
                if (destinationUnit == "kg")
                {
                    weight /= 35.274;
                }
                else if (destinationUnit == "g")
                {
                    weight *= 28.3495;
                }
                else if (destinationUnit == "lb")
                {
                    weight /= 16;
                }
                else if (destinationUnit == "oz")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {weight:0.0} {destinationUnit}");
                }
            }

            await command.RespondAsync($"{input} {weight:0.0} {destinationUnit}");
        }

        if (command.Data.Name == "volume")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();
            double volume = double.Parse(command.Data.Options.ElementAt(0).Value.ToString());
            string input = $"`{volume} {sourceUnit}:`  ";

            if (sourceUnit == "l")
            {
                if (destinationUnit == "l")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 1000;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.264172;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.106919;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.0284131;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.00416667;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 29.5735;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 202.884;
                }
            }
            else if (sourceUnit == "ml")
            {
                if (destinationUnit == "l")
                {
                    volume /= 1000;
                }
                else if (destinationUnit == "ml")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.000264172;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.000130772;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.0000492892;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.0000236588;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 0.33814;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 0.0692641;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 0.20094;
                }
            }
            else if (sourceUnit == "gal")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.264172;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 264.172;
                }
                else if (destinationUnit == "gal")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 2.25;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 4.92892;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 2.36588;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 33.814;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 202.884;
                }
            }
            else if (sourceUnit == "qt")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.106919;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 1069.19;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.00378541;
                }
                else if (destinationUnit == "qt")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 2.11338;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 1.05669;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 33.814;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 202.884;
                }
            }
            else if (sourceUnit == "pt")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.0284131;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 284.131;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.00284130;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.00131577;
                }
                else if (destinationUnit == "pt")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.0692641;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 29.5735;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 202.884;
                }
            }
            else if (sourceUnit == "cup")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.00416667;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 4166.67;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.00211338;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.00105669;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.0284131;
                }
                else if (destinationUnit == "cup")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 33.814;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 202.884;
                }
            }
            else if (sourceUnit == "fl oz")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.33814;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 33814;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.00295735;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.00147575;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.0284131;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.00416667;
                }
                else if (destinationUnit == "fl oz")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 0.0692641;
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 0.20094;
                }
            }
            else if (sourceUnit == "tbsp")
            {
                if (destinationUnit == "l")
                {
                    volume /= 0.676280;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 67.628;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.000692641;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.000284131;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.0284131;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.00416667;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 0.0692641;
                }
                else if (destinationUnit == "tbsp")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume *= 3.96563;
                }
            }
            else if (sourceUnit == "tsp")
            {
                if (destinationUnit == "l")
                {
                    volume /= 202.884;
                }
                else if (destinationUnit == "ml")
                {
                    volume *= 202.884;
                }
                else if (destinationUnit == "gal")
                {
                    volume *= 0.001;
                }
                else if (destinationUnit == "qt")
                {
                    volume *= 0.000202884;
                }
                else if (destinationUnit == "pt")
                {
                    volume *= 0.000284131;
                }
                else if (destinationUnit == "cup")
                {
                    volume *= 0.000416670;
                }
                else if (destinationUnit == "fl oz")
                {
                    volume *= 0.0692641;
                }
                else if (destinationUnit == "tbsp")
                {
                    volume *= 0.00676280;
                }
                else if (destinationUnit == "tsp")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
            }

            await command.RespondAsync($"{input} {volume:0.0} {destinationUnit}");
        }

        if (command.Data.Name == "speed")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();
            double speed = double.Parse(command.Data.Options.ElementAt(0).Value.ToString());
            string input = $"`{speed} {sourceUnit}:`  ";
            

            if (sourceUnit == "m/s")
            {
                if (destinationUnit == "m/s")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {speed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "km/h")
                {
                    speed *= 3.6;
                }
                else if (destinationUnit == "mph")
                {
                    speed *= 2.23694;
                }
                else if (destinationUnit == "knot")
                {
                    speed *= 1.94384;
                }
            }
            else if (sourceUnit == "km/h")
            {
                if (destinationUnit == "m/s")
                {
                    speed /= 3.6;
                }
                else if (destinationUnit == "km/h")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {speed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mph")
                {
                    speed /= 1.60934;
                }
                else if (destinationUnit == "knot")
                {
                    speed /= 1.852;
                }
            }
            else if (sourceUnit == "mph")
            {
                if (destinationUnit == "m/s")
                {
                    speed /= 2.23694;
                }
                else if (destinationUnit == "km/h")
                {
                    speed *= 1.60934;
                }
                else if (destinationUnit == "mph")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {speed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "knot")
                {
                    speed *= 1.15078;
                }
            }
            else if (sourceUnit == "knot")
            {
                if (destinationUnit == "m/s")
                {
                    speed /= 1.852;
                }
                else if (destinationUnit == "km/h")
                {
                    speed *= 1.852;
                }
                else if (destinationUnit == "mph")
                {
                    speed *= 1.15078;
                }
                else if (destinationUnit == "knot")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {speed:0.0} {destinationUnit}");
                }
            }

            await command.RespondAsync($"{input} {speed:0.0} {destinationUnit}");
        }
    }
}
