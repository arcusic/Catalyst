using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Catalyst.Common;
using Catalyst.Init;
using UnitsNet;
using System.Globalization;

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
#if DEBUG
            var dateTime = DateTime.UtcNow;
#endif

#if RELEASE
            string path = "/root/.config/ookla/build.txt";
            var dateTime = File.GetLastWriteTimeUtc(path);
#endif
            string build = dateTime.ToString("yyMMddHHmm");
            version = version.Replace(".0", "");
#if DEBUG
            string description = $":warning: `THIS IS A PRE-RELEASE VERSION.` :warning:\n\n" +
                $"`Catalyst Version:`  v{version}-alpha ({build})\n\n" +
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
                $"> Utilities Module - v{version}-alpha\n\n" +
                $"`Built On:` {dateTime} UTC";
#endif
#if RELEASE
            string description = $"`Catalyst Version:`  v{version} ({build})\n\n" +
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
                $"> Utilities Module - v{version}\n\n" +
                $"`Built On:` {dateTime} UTC";
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

#pragma warning disable CS8604 // Possible null reference argument.
            double inputTemp = double.Parse(command.Data.Options.First().Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
#pragma warning restore CS8604 // Possible null reference argument.

            string input = $"`{inputTemp}°{unit}:`  ";
            UnitsNet.Temperature temp;

            if (unit == "C")
            {
                temp = Temperature.From(inputTemp, UnitsNet.Units.TemperatureUnit.DegreeCelsius).ToUnit(UnitsNet.Units.TemperatureUnit.DegreeFahrenheit);
            }
            else
            {
                temp = Temperature.From(inputTemp, UnitsNet.Units.TemperatureUnit.DegreeFahrenheit).ToUnit(UnitsNet.Units.TemperatureUnit.DegreeCelsius);
            }
            await command.RespondAsync($"{input} {temp}");
        }

        if (command.Data.Name == "distance")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
            double inputDistance = double.Parse(command.Data.Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
#pragma warning restore CS8604 // Possible null reference argument.

            string input = $"`{inputDistance} {sourceUnit}:`  ";
            UnitsNet.Length distance;

            if (sourceUnit == "m")
            {
                if (destinationUnit == "m")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Meter).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "km")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Kilometer).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "mi")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Mile).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "ft")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Foot).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "yd")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Yard).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "in")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
                else if (destinationUnit == "cm")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Inch).ToUnit(UnitsNet.Units.LengthUnit.Centimeter);
                    await command.RespondAsync($"{input} {distance}");
                }
            }
            else if (sourceUnit == "cm")
            {
                if (destinationUnit == "m")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Meter);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "km")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Kilometer);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "mi")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Mile);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "ft")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Foot);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "yd")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Yard);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "in")
                {
                    distance = Length.From(inputDistance, UnitsNet.Units.LengthUnit.Centimeter).ToUnit(UnitsNet.Units.LengthUnit.Inch);
                    await command.RespondAsync($"{input} {distance}");
                }
                else if (destinationUnit == "cm")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputDistance:n2} {destinationUnit}");
                }
            }
        }

        if (command.Data.Name == "weight")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
            double inputWeight = double.Parse(command.Data.Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
#pragma warning restore CS8604 // Possible null reference argument.

            string input = $"`{inputWeight} {sourceUnit}:`  ";
            UnitsNet.Mass weight;

            if (sourceUnit == "kg")
            {
                if (destinationUnit == "kg")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputWeight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "g")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Kilogram).ToUnit(UnitsNet.Units.MassUnit.Gram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "lb")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Kilogram).ToUnit(UnitsNet.Units.MassUnit.Pound);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "oz")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Kilogram).ToUnit(UnitsNet.Units.MassUnit.Ounce);
                    await command.RespondAsync($"{input} {weight}");
                }
            }
            else if (sourceUnit == "g")
            {
                if (destinationUnit == "kg")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Gram).ToUnit(UnitsNet.Units.MassUnit.Kilogram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "g")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputWeight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "lb")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Gram).ToUnit(UnitsNet.Units.MassUnit.Pound);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "oz")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Gram).ToUnit(UnitsNet.Units.MassUnit.Ounce);
                    await command.RespondAsync($"{input} {weight}");
                }
            }
            else if (sourceUnit == "lb")
            {
                if (destinationUnit == "kg")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Pound).ToUnit(UnitsNet.Units.MassUnit.Kilogram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "g")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Pound).ToUnit(UnitsNet.Units.MassUnit.Gram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "lb")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputWeight:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "oz")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Pound).ToUnit(UnitsNet.Units.MassUnit.Ounce);
                    await command.RespondAsync($"{input} {weight}");
                }
            }
            else if (sourceUnit == "oz")
            {
                if (destinationUnit == "kg")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Ounce).ToUnit(UnitsNet.Units.MassUnit.Kilogram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "g")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Ounce).ToUnit(UnitsNet.Units.MassUnit.Gram);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "lb")
                {
                    weight = Mass.From(inputWeight, UnitsNet.Units.MassUnit.Ounce).ToUnit(UnitsNet.Units.MassUnit.Pound);
                    await command.RespondAsync($"{input} {weight}");
                }
                else if (destinationUnit == "oz")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputWeight:0.0} {destinationUnit}");
                }
            }
        }

        if (command.Data.Name == "volume")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
            double inputVolume = double.Parse(command.Data.Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
#pragma warning restore CS8604 // Possible null reference argument.

            string input = $"`{inputVolume} {sourceUnit}:`  ";
            UnitsNet.Volume volume;

            if (sourceUnit == "L")
            {
                if (destinationUnit == "L")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Liter).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "mL")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.Milliliter).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "gal")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsGallon).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "qt")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsQuart).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "pt")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsPint).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "cup")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsCustomaryCup).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "fl oz")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsOunce).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "tbsp")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "tsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTablespoon).ToUnit(UnitsNet.Units.VolumeUnit.UsTeaspoon);
                    await command.RespondAsync($"{input} {volume}");
                }
            }
            else if (sourceUnit == "tsp")
            {
                if (destinationUnit == "L")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.Liter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "mL")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.Milliliter);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "gal")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsGallon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "qt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsQuart);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "pt")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsPint);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "cup")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsCustomaryCup);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "fl oz")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsOunce);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tbsp")
                {
                    volume = Volume.From(inputVolume, UnitsNet.Units.VolumeUnit.UsTeaspoon).ToUnit(UnitsNet.Units.VolumeUnit.UsTablespoon);
                    await command.RespondAsync($"{input} {volume}");
                }
                else if (destinationUnit == "tsp")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputVolume:0.0} {destinationUnit}");
                }
            }
        }

        if (command.Data.Name == "speed")
        {
            string? sourceUnit = command.Data.Options.ElementAt(1).Value.ToString();
            string? destinationUnit = command.Data.Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
            double inputSpeed = double.Parse(command.Data.Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
#pragma warning restore CS8604 // Possible null reference argument.

            string input = $"`{inputSpeed} {sourceUnit}:`  ";
            UnitsNet.Speed speed;

            if (sourceUnit == "m/s")
            {
                if (destinationUnit == "m/s")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputSpeed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "km/h")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.CentimeterPerSecond).ToUnit(UnitsNet.Units.SpeedUnit.KilometerPerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "mph")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.CentimeterPerSecond).ToUnit(UnitsNet.Units.SpeedUnit.MilePerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "knot")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.CentimeterPerSecond).ToUnit(UnitsNet.Units.SpeedUnit.Knot);
                    await command.RespondAsync($"{input} {speed}");
                }
            }
            else if (sourceUnit == "km/h")
            {
                if (destinationUnit == "m/s")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.KilometerPerHour).ToUnit(UnitsNet.Units.SpeedUnit.MeterPerSecond);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "km/h")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputSpeed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "mph")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.KilometerPerHour).ToUnit(UnitsNet.Units.SpeedUnit.MilePerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "knot")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.KilometerPerHour).ToUnit(UnitsNet.Units.SpeedUnit.Knot);
                    await command.RespondAsync($"{input} {speed}");
                }
            }
            else if (sourceUnit == "mph")
            {
                if (destinationUnit == "m/s")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.MilePerHour).ToUnit(UnitsNet.Units.SpeedUnit.MeterPerSecond);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "km/h")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.MilePerHour).ToUnit(UnitsNet.Units.SpeedUnit.KilometerPerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "mph")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputSpeed:0.0} {destinationUnit}");
                }
                else if (destinationUnit == "knot")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.MilePerHour).ToUnit(UnitsNet.Units.SpeedUnit.Knot);
                    await command.RespondAsync($"{input} {speed}");
                }
            }
            else if (sourceUnit == "knot")
            {
                if (destinationUnit == "m/s")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.Knot).ToUnit(UnitsNet.Units.SpeedUnit.MeterPerSecond);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "km/h")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.Knot).ToUnit(UnitsNet.Units.SpeedUnit.KilometerPerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "mph")
                {
                    speed = Speed.From(inputSpeed, UnitsNet.Units.SpeedUnit.Knot).ToUnit(UnitsNet.Units.SpeedUnit.MilePerHour);
                    await command.RespondAsync($"{input} {speed}");
                }
                else if (destinationUnit == "knot")
                {
                    await command.RespondAsync($"Seriously... convert it yourself...\n{input} {inputSpeed:0.0} {destinationUnit}");
                }
            }
        }
    }
    
    public async Task ButtonHandler(SocketMessageComponent component)
    {
        
    }
}
