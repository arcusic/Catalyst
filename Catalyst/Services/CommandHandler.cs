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
            operatingSystem = Environment.OSVersion.ToString().Contains("Unix") ? "Unix" : Environment.OSVersion.ToString();

            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            version = version.Replace(".0", "");
#if DEBUG
            string description = $":warning: `THIS IS A PRE-RELEASE VERSION.` :warning:\n\n" +
                $"`Catalyst Version:`  v{version}-alpha (Build {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version})\n\n" +
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
                $"> Utilities Module - v{version} (Build 2208)\n\n";
#endif
#if RELEASE
            string description = $"`Catalyst Version:`  v{version} (Build {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version})\n\n" +
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
                $"> Utilities Module - v{version} (Build 2208)\n\n";
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

            if (sourceUnit == "L")
            {
                if (destinationUnit == "L")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {volume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mL")
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
            else if (sourceUnit == "mL")
            {
                if (destinationUnit == "L")
                {
                    volume /= 1000;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.264172;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.106919;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.0284131;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.00416667;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.33814;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 0.676280;
                }
                else if (destinationUnit == "mL")
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
                if (destinationUnit == "L")
                {
                    volume /= 202.884;
                }
                else if (destinationUnit == "mL")
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
    
    public async Task ButtonHandler(SocketMessageComponent component)
    {
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
                break;

            case "mute":
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
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "warn":
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
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "kick":
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
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "ban":
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
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "purge":
                embed.Title += " - Purge";
                embed.Description = "Deletes number of specified recent messages within the channel executed.\n\n" +
                    ":warning:  `User is optional.`  **Including a user will target their messages only!!!**  :warning:\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+purge # {@user | UserID}\n\n" +
                    "+purge 10\n" +
                    "+purge 25 @GHXST#2586\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "unmute":
                embed.Title += " - Removing Mutes";
                embed.Description = "Unmutes the specified user(s).\n\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+unmute @User | UserID\n\n" +
                    "+unmute @Tactical050#9264, @Catalyst#7894\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "unban":
                embed.Title += " - Removing Bans";
                embed.Description = "Removes a ban the specified user(s).\n\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+unban @User | UserID\n\n" +
                    "+unban @lovelxrd#7985\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "slowmode":
                embed.Title += " - Slow Mode";
                embed.Description = "Enables slow mode for a specified channel.\n\n" +
                    ":warning:  Duration must be between `1 second` and `6 hours`.  :warning:\n" +
                    ":information_source:  To disable Slow Mode use a time entry of `0`.  :information_source:\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+slowmode <#Channel_Name> #(s/m/h)\n\n" +
                    "+slowmode #┃chat 5m\n\n" +
                    "+slowmode #┃chat 0\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "lockdown":
                embed.Title += " - Lockdown";
                embed.Description = "This command has two variants, lockdown, and unlocking the server.\n\n" +
                    "`Locking Down the Server:`\n" +
                    ":warning:  `sc` will lockdown all channels.  `c` will lockdown the current channel.  :warning:" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+ld ?t sc | c\n\n" +
                    "+ld ?t c\n\n" +
                    "+ld ?t sc\n\n" +
                    "```\n" +
                    "`Endling a Server Lockdown:`\n" +
                    ":warning:  `sc` will unlock all channels.  `c` will unlock the current channel.  :warning:" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+ul ?t sc | c\n\n" +
                    "+ul ?t c\n\n" +
                    "+ul ?t sc\n\n" +
                    "```\n" +
                    "`Sending Status Updates:`\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+ld <Message> ?t update\n\n" +
                    "+ld Hello, we have had to lockdown this server due to an ongoing incident.  Updates will be provided shortly... ?t update\n\n" +
                    "+ld Server staff are still actively working on the issue.  An update will be provided shortly... ?t update\n\n" +
                    "+ld The incident has been resolved.  Please be patient while Staff finish up and unlock the server. ?t update\n\n" +
                    "```\n\n" +
                    "`Troubleshooting Lockdown Issues:`\n" +
                    "```\n" +
                    "Command Syntax:\n" +
                    "+tshoot lockdown\n\n" +
                    "```\n\n" +
                    "See docs included with each command for details.\n" +
                    "`Please click one of the buttons for command details.`\n\n" +
                    "`Close:`  Deletes this message.";

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                break;

            case "close":
                var message = component.Message;
                await message.DeleteAsync();
                break;
        }
    }
}
