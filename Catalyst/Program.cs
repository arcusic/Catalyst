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
using System.Net;
using System.Security.Policy;
using System.Text.Json;

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
    TotalShards = null,
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

    List<ApplicationCommandProperties> globalApplicationCommandProperties = new();
    List<ApplicationCommandProperties> tacticalApplicationCommandProperties = new();

    //Start Conversion Module
    SlashCommandBuilder conversion = new();
    conversion.WithName("conversion").WithDescription("Converts Units");
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Preparing to provision Slash Command {conversion.Name}...");

    SlashCommandOptionBuilder temperatureConversion = new();
    temperatureConversion.WithName("temperature").WithDescription("Temperature Conversion").WithType(ApplicationCommandOptionType.SubCommand)
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
            .WithType(ApplicationCommandOptionType.String));

    conversion.AddOptions(temperatureConversion);
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {temperatureConversion.Name} provisioned to {conversion.Name}.");

    SlashCommandOptionBuilder distanceConversion = new();
    distanceConversion.WithName("distance").WithDescription("Distance Conversion").WithType(ApplicationCommandOptionType.SubCommand)
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
            .WithType(ApplicationCommandOptionType.String));

    conversion.AddOptions(distanceConversion);
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {distanceConversion.Name} provisioned to {conversion.Name}.");

    SlashCommandOptionBuilder weightConversion = new();
    weightConversion.WithName("weight").WithDescription("Weight Conversion").WithType(ApplicationCommandOptionType.SubCommand)
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
            .WithType(ApplicationCommandOptionType.String));

    conversion.AddOptions(weightConversion);
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {weightConversion.Name} provisioned to {conversion.Name}.");

    SlashCommandOptionBuilder volumeConversion = new();
    volumeConversion.WithName("volume").WithDescription("Volume Conversion").WithType(ApplicationCommandOptionType.SubCommand)
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
            .WithType(ApplicationCommandOptionType.String));

    conversion.AddOptions(volumeConversion);
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {volumeConversion.Name} provisioned to {conversion.Name}.");

    SlashCommandOptionBuilder speedConversion = new();
    speedConversion.WithName("speed").WithDescription("Speed Conversion").WithType(ApplicationCommandOptionType.SubCommand)
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
            .WithType(ApplicationCommandOptionType.String));

    conversion.AddOptions(speedConversion);
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {speedConversion.Name} provisioned to {conversion.Name}.");

    globalApplicationCommandProperties.Add(conversion.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {conversion.Name} provisioned to Global Commands.");
    //End Conversion Module

    //Start About Module
    SlashCommandBuilder about = new SlashCommandBuilder();
    about.WithName("about").WithDescription("About The Catalyst");
    globalApplicationCommandProperties.Add(about.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {about.Name} provisioned to Global Commands.");

    SlashCommandBuilder changeLog = new SlashCommandBuilder();
    changeLog.WithName("release_notes").WithDescription("Release Notes");
    globalApplicationCommandProperties.Add(changeLog.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {changeLog.Name} provisioned to Global Commands.");

    SlashCommandBuilder help = new SlashCommandBuilder();
    help.WithName("help").WithDescription("The Catalyst Documentation");
    globalApplicationCommandProperties.Add(help.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {help.Name} provisioned to Global Commands.");
    //End About Module

    //Start Utility Module
    SlashCommandBuilder hc = new();
    hc.WithName("status").WithDescription("The Catalyst Status");
    globalApplicationCommandProperties.Add(hc.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {hc.Name} provisioned to Global Commands.");

    SlashCommandBuilder epo = new();
    epo.WithName("emergency_power_off").WithDescription("Emergency Power Off");
    globalApplicationCommandProperties.Add(epo.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {epo.Name} provisioned to Global Commands.");

    SlashCommandBuilder tacticraft_latest_log = new();
    tacticraft_latest_log.WithName("tacticraft_latest_log").WithDescription("Get the latest logs from Tacticraft");
    tacticalApplicationCommandProperties.Add(tacticraft_latest_log.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {tacticraft_latest_log.Name} provisioned to Guild (994625404243546292) Commands.");

    SlashCommandBuilder tacticraft_whitelist = new();
    SlashCommandOptionBuilder tacticraft_whitelist_options = new();
    tacticraft_whitelist_options.WithName("minecraft_username").WithDescription("Minecraft Username").WithRequired(true).WithType(ApplicationCommandOptionType.String);
    tacticraft_whitelist.WithName("tacticraft_whitelist").WithDescription("Whitelist Account for Tacticraft").AddOptions(tacticraft_whitelist_options);
    tacticalApplicationCommandProperties.Add(tacticraft_whitelist.Build());
    await Logger.Log(LogSeverity.Info, "CMDProvisioned", $"Slash Command {tacticraft_whitelist.Name} provisioned to Guild (994625404243546292) Commands.");
    //End Utility Module

    client.ShardReady += async shard =>
    {
        var tactical = shard.GetGuild(994625404243546292);

        await Logger.Log(LogSeverity.Info, "GLOBAL_BLD", $"Processing Global Application Commands...");
        await shard.BulkOverwriteGlobalApplicationCommandsAsync(globalApplicationCommandProperties.ToArray());
        await Logger.Log(LogSeverity.Info, "GLOBAL_BLD", $"Completed Global Application Commands.");

        await Logger.Log(LogSeverity.Info, "GUILD_BLD", $"Processing Guild Application Commands...");
        await Logger.Log(LogSeverity.Info, "GUILD_BLD", $"Processing Tactical Commands...");
        await tactical.BulkOverwriteApplicationCommandAsync(tacticalApplicationCommandProperties.ToArray());
        await Logger.Log(LogSeverity.Info, "GUILD_BLD", $"Completed Tactical Commands.");
        await Logger.Log(LogSeverity.Info, "GUILD_BLD", $"Completed Guild Application Commands.");

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

#if RELEASE
    if (!File.Exists("/root/.config/ookla/build.txt"))
    {
        File.WriteAllText("/root/.config/ookla/build.txt", DateTime.UtcNow.ToString());
    }
#endif

#if DEBUG
    await client.SetGameAsync($"v{version}-alpha");
    await client.SetStatusAsync(UserStatus.DoNotDisturb);

    
#endif

#if RELEASE
    await client.SetGameAsync($"v{version}");
    await client.SetStatusAsync(UserStatus.Online);
#endif

    var jsonString = await File.ReadAllTextAsync("appsettings.json");
    var appSettings = JsonDocument.Parse(jsonString)!;

    var heartbeatSettings = appSettings.RootElement.GetProperty("KeyVault").EnumerateObject();
    await Logger.Log(LogSeverity.Debug, $"JSONImported", "JSON file has been successfully imported... Processing.");

    string heartbeat = heartbeatSettings
        .Where(onexs => onexs.Name == "Heartbeat")
        .Select(onexs => onexs.Value)
        .FirstOrDefault()
        .ToString();

    var url = secretClient.GetSecret(heartbeat);

    await Logger.Log(LogSeverity.Debug, "UPSUserObtained", $"Successfully obtained UPS User from Azure Key Vault.");

    var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

    while (await timer.WaitForNextTickAsync())
    {
        using var wb = new HttpClient();
        using HttpResponseMessage response = await wb.GetAsync(url.Value.Value);
        response.EnsureSuccessStatusCode();
    }
    
    // Wait infinitely so your bot actually stays connected.
    await Task.Delay(Timeout.Infinite);
}
