using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Catalyst.Common;
using Catalyst.Init;
using Catalyst.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

// Initialize Local Configuration File
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

// Initialize Azure KeyVault
var secretClient = new SecretClient(new Uri($"https://{config.GetRequiredSection("KeyVault")["KeyVaultName"]}.vault.azure.net"), new ClientSecretCredential(config.GetRequiredSection("KeyVault")["AzureADTennantId"], config.GetRequiredSection("KeyVault")["AzureADClientId"], config.GetRequiredSection("KeyVault")["AzureADClientSecret"]));
await Logger.Log(LogSeverity.Debug, "SecretClientConfigured", $"Configured Azure Key Vault client to connect to {secretClient.VaultUri}.");

var token = secretClient.GetSecret(config.GetRequiredSection("KeyVault")["SecretName"]);
await Logger.Log(LogSeverity.Debug, "AuthTokenObtained", $"Successfully obtained token from Azure Key Vault. Secret ID: {token.Value.Id}");

var client = new DiscordShardedClient(new DiscordSocketConfig
{
    LogLevel = LogSeverity.Debug,
    MessageCacheSize = 1000,
    TotalShards = 1,
    GatewayIntents = GatewayIntents.All,
    AlwaysDownloadUsers = true,
    DefaultRetryMode = RetryMode.AlwaysRetry
});

var commands = new CommandService(new CommandServiceConfig
{
    DefaultRunMode = RunMode.Async,
    LogLevel = LogSeverity.Debug,
    CaseSensitiveCommands = true
});

// Setup your DI container.
Bootstrapper.Init();
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(commands);
Bootstrapper.RegisterType<ICommandHandler, CommandHandler>();
Bootstrapper.RegisterInstance(config);

await MainAsync();

async Task MainAsync()
{
    await Bootstrapper.ServiceProvider.GetRequiredService<ICommandHandler>().InitializeAsync();

    //Start Conversion Module
    var temperatureConversion = new SlashCommandBuilder()
        .WithName("temperature")
        .WithDescription("Temperature Conversion")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("temp")
            .WithDescription("Temperature")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Number))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Celsius", "C")
            .AddChoice("Fahrenheit", "F")
            .WithType(ApplicationCommandOptionType.String))
        .Build();

    var distanceConversion = new SlashCommandBuilder()
        .WithName("distance")
        .WithDescription("Distance Conversion")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("distance")
            .WithDescription("Distance")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Number))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("source_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilometers", "km")
            .AddChoice("Meters", "m")
            .AddChoice("Centimeters", "cm")
            .AddChoice("Miles", "mi")
            .AddChoice("Yards", "yd")
            .AddChoice("Feet", "ft")
            .AddChoice("Inches", "in")
            .WithType(ApplicationCommandOptionType.String))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("destination_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilometers", "km")
            .AddChoice("Meters", "m")
            .AddChoice("Centimeters", "cm")
            .AddChoice("Miles", "mi")
            .AddChoice("Yards", "yd")
            .AddChoice("Feet", "ft")
            .AddChoice("Inches", "in")
            .WithType(ApplicationCommandOptionType.String))
        .Build();
    
    var weightConversion = new SlashCommandBuilder()
        .WithName("weight")
        .WithDescription("Weight Conversion")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("weight")
            .WithDescription("Weight")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Number))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("source_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilograms", "kg")
            .AddChoice("Grams", "g")
            .AddChoice("Milligrams", "mg")
            .AddChoice("Pounds", "lb")
            .AddChoice("Ounces", "oz")
            .WithType(ApplicationCommandOptionType.String))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("destination_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilograms", "kg")
            .AddChoice("Grams", "g")
            .AddChoice("Milligrams", "mg")
            .AddChoice("Pounds", "lb")
            .AddChoice("Ounces", "oz")
            .WithType(ApplicationCommandOptionType.String))
        .Build();

    var volumeConversion = new SlashCommandBuilder()
        .WithName("volume")
        .WithDescription("Volume Conversion")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("volume")
            .WithDescription("Volume")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Number))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("source_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Liters", "L")
            .AddChoice("Milliliters", "mL")
            .AddChoice("Gallons", "gal")
            .AddChoice("Quarts", "qt")
            .AddChoice("Pints", "pt")
            .AddChoice("Cups", "cup")
            .AddChoice("Fluid Ounces", "fl oz")
            .AddChoice("Tablespoons", "tbsp")
            .AddChoice("Teaspoons", "tsp")
            .WithType(ApplicationCommandOptionType.String))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("destination_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Liters", "L")
            .AddChoice("Milliliters", "mL")
            .AddChoice("Gallons", "gal")
            .AddChoice("Quarts", "qt")
            .AddChoice("Pints", "pt")
            .AddChoice("Cups", "cup")
            .AddChoice("Fluid Ounces", "fl oz")
            .AddChoice("Tablespoons", "tbsp")
            .AddChoice("Teaspoons", "tsp")
            .WithType(ApplicationCommandOptionType.String))
        .Build();
    
    var speedConversion = new SlashCommandBuilder()
        .WithName("speed")
        .WithDescription("Speed Conversion")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("speed")
            .WithDescription("Speed")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Number))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("source_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilometers per hour", "km/h")
            .AddChoice("Meters per second", "m/s")
            .AddChoice("Miles per hour", "mph")
            .AddChoice("Knots", "kn")
            .WithType(ApplicationCommandOptionType.String))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("destination_unit")
            .WithDescription("Unit of Measurement")
            .WithRequired(true)
            .AddChoice("Kilometers per hour", "km/h")
            .AddChoice("Meters per second", "m/s")
            .AddChoice("Miles per hour", "mph")
            .AddChoice("Knots", "kn")
            .WithType(ApplicationCommandOptionType.String))
        .Build();
    //End Conversion Module

    //Start About Module
    var about = new SlashCommandBuilder()
        .WithName("about")
        .WithDescription("About The Catalyst")
        .Build();

    var changeLog = new SlashCommandBuilder()
        .WithName("release_notes")
        .WithDescription("Display Release Notes")
        .Build();

    var help = new SlashCommandBuilder()
        .WithName("help")
        .WithDescription("Help for all Context Commands")
        .Build();
    //End About Module

    client.ShardReady += async shard =>
    {
        await shard.CreateGlobalApplicationCommandAsync(about);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {about.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(changeLog);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {changeLog.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(help);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {help.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(temperatureConversion);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {temperatureConversion.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(distanceConversion);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {distanceConversion.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(weightConversion);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {weightConversion.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(volumeConversion);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {volumeConversion.Name} is built and ready!");

        await shard.CreateGlobalApplicationCommandAsync(speedConversion);
        await Logger.Log(LogSeverity.Info, "CMDBuilt", $"Slash Command {speedConversion.Name} is built and ready!");

        await Logger.Log(LogSeverity.Info, "ShardReady", $"Shard Number {shard.ShardId} is connected and ready!");
    };

    // Login and connect.    
    if (string.IsNullOrWhiteSpace(token.Value.Value))
    {
        await Logger.Log(LogSeverity.Error, $"{nameof(Program)} | {nameof(MainAsync)}", "Token is null or empty.");
        return;
    }

    await client.LoginAsync(TokenType.Bot, token.Value.Value);
    await Logger.Log(LogSeverity.Debug, $"Client{client.LoginState}", $"Discord Presence: {client.Status}.");
    
    await client.StartAsync();
    await client.DownloadUsersAsync(guilds: client.Guilds);

    await Logger.Log(LogSeverity.Debug, $"ClientReady", $"Client is connected to Discord.");

    string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
    version = version.Replace(".0", "");


#if DEBUG
    await client.SetGameAsync($"v{version}-alpha");
    await client.SetStatusAsync(UserStatus.DoNotDisturb);
#endif

#if RELEASE
    await client.SetGameAsync($"v{version}");
    await client.SetStatusAsync(UserStatus.Online);
#endif
    
    // Wait infinitely so your bot actually stays connected.
    await Task.Delay(Timeout.Infinite);
}
