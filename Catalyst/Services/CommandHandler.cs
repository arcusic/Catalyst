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
