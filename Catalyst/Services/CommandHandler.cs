using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Catalyst.Common;
using Catalyst.Init;
using UnitsNet;
using System.Globalization;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.Json;
using System.Text;
using Renci.SshNet;
using Microsoft.Extensions.Configuration;

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
            _ = await _commands.ExecuteAsync(context, markPos, Bootstrapper.ServiceProvider);
        }
    }

    public async Task SlashCommandHandler(SocketSlashCommand command)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var secretClient = new SecretClient(new Uri($"https://{config.GetRequiredSection("KeyVault")["KeyVaultName"]}.vault.azure.net"), new ClientSecretCredential(config.GetRequiredSection("KeyVault")["AzureADTennantId"], config.GetRequiredSection("KeyVault")["AzureADClientId"], config.GetRequiredSection("KeyVault")["AzureADClientSecret"]));
        await Logger.Log(LogSeverity.Debug, "SecretClientConfigured", $"Configured Azure Key Vault client to connect to {secretClient.VaultUri}.");

        if (command.Data.Name == "post_role_message")
        {
            var whiteCheckMark = new Emoji("\u2705");
            var redX = new Emoji("\u274C");
            var denied = new Emoji("\uD83D\uDEAB");

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.CommandName} from the {command.Channel.Name} channel.");

            if (command.Channel.Id == 996886356934533260)
            {
                await command.Channel.TriggerTypingAsync();

                var embed = new EmbedBuilder
                {
                    Title = "Review Updated Roles",
                    Description = "__**WARNING:**__ This command will `replace` the current message in #roles.  This cannot be undone.",
                    Color = Color.Red,
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
                    },
                };

                var buttons = new ComponentBuilder()
                    .WithButton("Proceed", "role_proceed", ButtonStyle.Success)
                    .WithButton("Abort", "role_abort", ButtonStyle.Danger)
                    .Build();

                await command.RespondAsync(embed: embed.Build(), components: buttons, ephemeral: true);
            }
            else
            {
                await command.RespondAsync(":no_entry:  ***UNAUTHORIZED***  :no_entry:\n" +
                "You have attempted to execute a privledged command without propper permissions.\n\n" +
                "__**WARNING:**__  This incident has been logged!\n" +
                "*Further attempts to execute a privledged command without authorization may lead to additional action.*", ephemeral: true);
            }
        }
        if (command.Data.Name == "tacticraft_whitelist")
        {
            var whiteCheckMark = new Emoji("\u2705");
            var redX = new Emoji("\u274C");
            var denied = new Emoji("\uD83D\uDEAB");

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.CommandName} from the {command.Channel.Name} channel.");

            if (command.GuildId == 994625404243546292)
            {
                await command.Channel.TriggerTypingAsync();
                string? mcUser = command.Data.Options.ElementAt(0).Value.ToString();

                var embed = new EmbedBuilder
                {
                    Title = $"Tacticraft Terms of Service - {mcUser}",
                    Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                    "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                    "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n `IF THE USER NAME LISTED ABOVE IS NOT CORRECT, CLICK ABORT.`",
                    Color = Color.Red,
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
                    },
                };

                var buttons = new ComponentBuilder()
                    .WithButton("I Agree", "agree", ButtonStyle.Success)
                    .WithButton("Abort", "abort", ButtonStyle.Danger)
                    .Build();

                await command.RespondAsync(embed: embed.Build(), components: buttons, ephemeral: true);
            }
            else
            {
                await command.RespondAsync(":no_entry:  ***UNAUTHORIZED***  :no_entry:\n" +
                "You have attempted to execute a privledged command without propper permissions.\n\n" +
                "__**WARNING:**__  This incident has been logged!\n" +
                "*Further attempts to execute a privledged command without authorization may lead to additional action.*", ephemeral: true);
            }
        }

        if (command.Data.Name == "tacticraft_latest_log")
        {
            var guild = _client.GetGuild(994625404243546292);
            var role = guild.GetRole(1090505454435708938);
            var user = _client.GetUser(command.User.Id);
            var roleMember = role.Members.Where(rm => rm.Id == user.Id).FirstOrDefault();

            if (roleMember != null)
            {
                var jsonString = await File.ReadAllTextAsync("appsettings.json");
                var appSettings = JsonDocument.Parse(jsonString)!;

                var keyVaultSettings = appSettings.RootElement.GetProperty("KeyVault").EnumerateObject();
                var snmpSettings = appSettings.RootElement.GetProperty("SNMP").EnumerateObject();
                var hardwareSettings = appSettings.RootElement.GetProperty("Hardware").EnumerateObject();
                var powerSettings = appSettings.RootElement.GetProperty("PowerAlert").EnumerateObject();
                await Logger.Log(LogSeverity.Debug, $"JSONImported", "JSON file has been successfully imported... Processing.");

                string keyVault = keyVaultSettings
                    .Where(onexs => onexs.Name == "KeyVaultName")
                    .Select(onexs => onexs.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADTennantId = keyVaultSettings
                    .Where(ascended => ascended.Name == "AzureADTennantId")
                    .Select(ascended => ascended.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADClientId = keyVaultSettings
                    .Where(goblino => goblino.Name == "AzureADClientId")
                    .Select(goblino => goblino.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADClientSecret = keyVaultSettings
                    .Where(gremlin => gremlin.Name == "AzureADClientSecret")
                    .Select(gremlin => gremlin.Value)
                    .FirstOrDefault()
                    .ToString();

                string powerUserInfo = powerSettings
                    .Where(onexs => onexs.Name == "MinecraftUser")
                    .Select(onexs => onexs.Value)
                    .FirstOrDefault()
                    .ToString();

                string powerPassInfo = powerSettings
                    .Where(catalyst => catalyst.Name == "MinecraftPass")
                    .Select(catalyst => catalyst.Value)
                    .FirstOrDefault()
                    .ToString();

                string hwDNS01 = hardwareSettings
                    .Where(kijmix => kijmix.Name == "MinecraftHost")
                    .Select(kijmix => kijmix.Value)
                    .FirstOrDefault()
                    .ToString();

                var powerUser = secretClient.GetSecret(powerUserInfo);
                await Logger.Log(LogSeverity.Debug, "UPSUserObtained", $"Successfully obtained UPS User from Azure Key Vault.");

                var powerPass = secretClient.GetSecret(powerPassInfo);
                await Logger.Log(LogSeverity.Debug, "UPSPassObtained", $"Successfully obtained UPS Pass from Azure Key Vault.");

                var dns01 = secretClient.GetSecret(hwDNS01);
                await Logger.Log(LogSeverity.Debug, "DNS01IPObtained", $"Successfully obtained DNS01 IP Address from Azure Key Vault.");

                var connectionInfo = new ConnectionInfo(dns01.Value.Value, powerUser.Value.Value,
                    new PasswordAuthenticationMethod(powerUser.Value.Value, powerPass.Value.Value));

                using var sftpClient = new SftpClient(connectionInfo);

                sftpClient.Connect();
                Stream latestLog = sftpClient.OpenRead("/opt/mscs/worlds/tacticraft/logs/latest.log");

                await command.RespondWithFileAsync(latestLog, "latest.log", ephemeral: true);
                sftpClient.Disconnect();
                sftpClient.Dispose();
            }
            else
            {
                await command.RespondAsync(":no_entry:  ***UNAUTHORIZED***  :no_entry:\n" +
                "You have attempted to execute a privledged command without propper permissions.\n\n" +
                "__**WARNING:**__  This incident has been logged!\n" +
                "*Further attempts to execute a privledged command without authorization may lead to additional action.*", ephemeral: true);
            }
        }

        if (command.Data.Name == "emergency_power_off")
        {
            var whiteCheckMark = new Emoji("\u2705");
            var redX = new Emoji("\u274C");
            var denied = new Emoji("\uD83D\uDEAB");

            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.CommandName} from the {command.Channel.Name} channel.");

            if (command.User.Id == 162600879948562432)
            {
                await command.Channel.TriggerTypingAsync();

                var embed = new EmbedBuilder
                {
                    Title = "Emergency Power Off",
                    Description = "__**WARNING:**__ This command will `Power Off` the Servers within the Enclosure!",
                    Color = Color.Red,
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
                    },
                };

                var buttons = new ComponentBuilder()
                    .WithButton("Proceed", "proceed", ButtonStyle.Success)
                    .WithButton("Abort", "abort", ButtonStyle.Danger)
                    .Build();

                await command.RespondAsync(embed: embed.Build(), components: buttons, ephemeral: true);
            }
            else
            {
                await command.RespondAsync(":no_entry:  ***UNAUTHORIZED***  :no_entry:\n" +
                "You have attempted to execute a privledged command without propper permissions.\n\n" +
                "__**WARNING:**__  This incident has been logged!\n" +
                "*Further attempts to execute a privledged command without authorization may lead to additional action.*", ephemeral: true);
            }
        }

        if (command.Data.Name == "status")
        {
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] CommandReceived", $"{command.User.Username}#{command.User.DiscriminatorValue} has invoked {command.CommandName} from the {command.Channel.Name} channel.");

            var typingState = command.Channel.EnterTypingState();

            await command.RespondAsync($"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Preparing for execution...", ephemeral: true);
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"'Executing infrastructure health check... please wait.' in the {command.Channel.Name} channel.");
            await command.Channel.TriggerTypingAsync();

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Parsing required information...");
            var jsonString = await File.ReadAllTextAsync("appsettings.json");
            var appSettings = JsonDocument.Parse(jsonString)!;
            var keyVaultSettings = appSettings.RootElement.GetProperty("KeyVault").EnumerateObject();
            var snmpSettings = appSettings.RootElement.GetProperty("SNMP").EnumerateObject();
            var topologySettings = appSettings.RootElement.GetProperty("Topology").EnumerateObject();
            var hardwareSettings = appSettings.RootElement.GetProperty("Hardware").EnumerateObject();
            await Logger.Log(LogSeverity.Debug, $"JSONImported", "JSON file has been successfully imported... Processing.");

            string keyVault = keyVaultSettings
                .Where(onexs => onexs.Name == "KeyVaultName")
                .Select(onexs => onexs.Value)
                .FirstOrDefault()
                .ToString();

            string azureADTennantId = keyVaultSettings
                .Where(ascended => ascended.Name == "AzureADTennantId")
                .Select(ascended => ascended.Value)
                .FirstOrDefault()
                .ToString();

            string azureADClientId = keyVaultSettings
                .Where(goblino => goblino.Name == "AzureADClientId")
                .Select(goblino => goblino.Value)
                .FirstOrDefault()
                .ToString();

            string azureADClientSecret = keyVaultSettings
                .Where(gremlin => gremlin.Name == "AzureADClientSecret")
                .Select(gremlin => gremlin.Value)
                .FirstOrDefault()
                .ToString();

            string upsIPSecret = snmpSettings
                .Where(jannik => jannik.Name == "UPSIPAddress")
                .Select(jannik => jannik.Value)
                .FirstOrDefault()
                .ToString();

            string upsSnmpCommunity = snmpSettings
                .Where(tactical => tactical.Name == "UPSCommunity")
                .Select(tactical => tactical.Value)
                .FirstOrDefault()
                .ToString();

            string upsSnmpPort = snmpSettings
                .Where(ghxst => ghxst.Name == "UPSSNMPPort")
                .Select(ghxst => ghxst.Value)
                .FirstOrDefault()
                .ToString();

            string topGW = topologySettings
                .Where(kxunna => kxunna.Name == "GW")
                .Select(kxunna => kxunna.Value)
                .FirstOrDefault()
                .ToString();

            string topAGG1 = topologySettings
                .Where(nova => nova.Name == "AggP")
                .Select(nova => nova.Value)
                .FirstOrDefault()
                .ToString();

            string topAGG2 = topologySettings
                .Where(frozendion => frozendion.Name == "AggA2")
                .Select(frozendion => frozendion.Value)
                .FirstOrDefault()
                .ToString();

            string topCore1 = topologySettings
                .Where(lenx => lenx.Name == "CoreSW1")
                .Select(lenx => lenx.Value)
                .FirstOrDefault()
                .ToString();

            string topCore2 = topologySettings
                .Where(xndrops => xndrops.Name == "CoreSW2")
                .Select(xndrops => xndrops.Value)
                .FirstOrDefault()
                .ToString();

            string topACC1 = topologySettings
                .Where(altercore => altercore.Name == "AccSW1")
                .Select(altercore => altercore.Value)
                .FirstOrDefault()
                .ToString();

            string topAP1 = topologySettings
                .Where(holygrail => holygrail.Name == "AP01")
                .Select(holygrail => holygrail.Value)
                .FirstOrDefault()
                .ToString();

            string topAP2 = topologySettings
                .Where(dxxm => dxxm.Name == "AP02")
                .Select(dxxm => dxxm.Value)
                .FirstOrDefault()
                .ToString();

            string topLTE = topologySettings
                .Where(mxdvs => mxdvs.Name == "LTE")
                .Select(mxdvs => mxdvs.Value)
                .FirstOrDefault()
                .ToString();

            string topPDU = topologySettings
                .Where(tikva => tikva.Name == "PDU")
                .Select(tikva => tikva.Value)
                .FirstOrDefault()
                .ToString();

            string topONT = topologySettings
                .Where(simply => simply.Name == "ONT")
                .Select(simply => simply.Value)
                .FirstOrDefault()
                .ToString();

            string hwDNS01 = hardwareSettings
                .Where(kijmix => kijmix.Name == "DNS01")
                .Select(kijmix => kijmix.Value)
                .FirstOrDefault()
                .ToString();

            string hwDNS02 = hardwareSettings
                .Where(howly => howly.Name == "DNS02")
                .Select(howly => howly.Value)
                .FirstOrDefault()
                .ToString();

            string hwESX01 = hardwareSettings
                .Where(cryes => cryes.Name == "ESX01")
                .Select(cryes => cryes.Value)
                .FirstOrDefault()
                .ToString();

            string hwESX02 = hardwareSettings
                .Where(mac56 => mac56.Name == "ESX02")
                .Select(mac56 => mac56.Value)
                .FirstOrDefault()
                .ToString();

            await Logger.Log(LogSeverity.Debug, $"JSONParsed", "JSON file has been successfully parsed.");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Retreiving Secrets from Azure KeyVault...");

            var upsIPAddress = secretClient.GetSecret(upsIPSecret);
            await Logger.Log(LogSeverity.Debug, "SNMPAddressObtained", $"Successfully obtained SNMP Address from Azure Key Vault.");

            var snmpCommunity = secretClient.GetSecret(upsSnmpCommunity);
            await Logger.Log(LogSeverity.Debug, "SNMPCommunityObtained", $"Successfully obtained SNMP Community from Azure Key Vault.");

            var snmpPort = secretClient.GetSecret(upsSnmpPort);
            await Logger.Log(LogSeverity.Debug, "SNMPPortObtained", $"Successfully obtained SNMP Port from Azure Key Vault.");

            var gatewayIP = secretClient.GetSecret(topGW);
            await Logger.Log(LogSeverity.Debug, "GatewayIPObtained", $"Successfully obtained Gateway IP Address from Azure Key Vault.");

            var aggA1IP = secretClient.GetSecret(topAGG1);
            await Logger.Log(LogSeverity.Debug, "AGGA1IPObtained", $"Successfully obtained AGG-A1 IP Address from Azure Key Vault.");

            var aggA2IP = secretClient.GetSecret(topAGG2);
            await Logger.Log(LogSeverity.Debug, "AGGA2IPObtained", $"Successfully obtained AGG-A2 IP Address from Azure Key Vault.");

            var coreSW1IP = secretClient.GetSecret(topCore1);
            await Logger.Log(LogSeverity.Debug, "CORESW1IPObtained", $"Successfully obtained CORE-SW1 IP Address from Azure Key Vault.");

            var coreSW2IP = secretClient.GetSecret(topCore2);
            await Logger.Log(LogSeverity.Debug, "CORESW2IPObtained", $"Successfully obtained CORE-SW2 IP Address from Azure Key Vault.");

            var accSW1IP = secretClient.GetSecret(topACC1);
            await Logger.Log(LogSeverity.Debug, "ACCSW1IPObtained", $"Successfully obtained ACC-SW1 IP Address from Azure Key Vault.");

            var ap1IP = secretClient.GetSecret(topAP1);
            await Logger.Log(LogSeverity.Debug, "AP1IPObtained", $"Successfully obtained AP-1 IP Address from Azure Key Vault.");

            var ap2IP = secretClient.GetSecret(topAP2);
            await Logger.Log(LogSeverity.Debug, "AP2IPObtained", $"Successfully obtained AP-2 IP Address from Azure Key Vault.");

            var lteIP = secretClient.GetSecret(topLTE);
            await Logger.Log(LogSeverity.Debug, "LTEIPObtained", $"Successfully obtained LTE IP Address from Azure Key Vault.");

            var pduIP = secretClient.GetSecret(topPDU);
            await Logger.Log(LogSeverity.Debug, "PDUIPObtained", $"Successfully obtained PDU IP Address from Azure Key Vault.");

            var ontIP = secretClient.GetSecret(topONT);
            await Logger.Log(LogSeverity.Debug, "ONTIPObtained", $"Successfully obtained ONT IP Address from Azure Key Vault.");

            var esx01IP = secretClient.GetSecret(hwESX01);
            await Logger.Log(LogSeverity.Debug, "ESX01IPObtained", $"Successfully obtained ESX01 IP Address from Azure Key Vault.");

            var esx02IP = secretClient.GetSecret(hwESX02);
            await Logger.Log(LogSeverity.Debug, "ESX02IPObtained", $"Successfully obtained ESX02 IP Address from Azure Key Vault.");

            var dns01IP = secretClient.GetSecret(hwDNS01);
            await Logger.Log(LogSeverity.Debug, "DNS01IPObtained", $"Successfully obtained DNS01 IP Address from Azure Key Vault.");

            var dns02IP = secretClient.GetSecret(hwDNS02);
            await Logger.Log(LogSeverity.Debug, "DNS02IPObtained", $"Successfully obtained DNS02 IP Address from Azure Key Vault.");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Gathering Environmental Information...");
            var tempResult = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(upsIPAddress.Value.Value), Convert.ToInt32(snmpPort.Value.Value)),
                new OctetString(snmpCommunity.Value.Value),
                new List<Variable> { new Variable(new ObjectIdentifier(".1.3.6.1.4.1.850.1.1.3.3.3.1.1.2.2")) },
                60000);
            await Logger.Log(LogSeverity.Debug, "UPSTempObtained", $"Successfully obtained Temperature from UPS. {tempResult[0].Data.ToString()}");

            var humResult = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(upsIPAddress.Value.Value), Convert.ToInt32(snmpPort.Value.Value)),
                new OctetString(snmpCommunity.Value.Value),
                new List<Variable> { new Variable(new ObjectIdentifier(".1.3.6.1.4.1.850.1.1.3.3.3.2.1.1.2")) },
                60000);
            await Logger.Log(LogSeverity.Debug, "UPSHumidObtained", $"Successfully obtained Humidity from UPS. {humResult[0].Data.ToString()}");

            var capResult = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(upsIPAddress.Value.Value), Convert.ToInt32(snmpPort.Value.Value)),
                new OctetString(snmpCommunity.Value.Value),
                new List<Variable> { new Variable(new ObjectIdentifier(".1.3.6.1.4.1.850.1.1.3.1.3.1.1.1.4.1")) },
                60000);
            await Logger.Log(LogSeverity.Debug, "UPSCapObtained", $"Successfully obtained Battery Capacity from UPS. {capResult[0].Data.ToString()}");

            var timeResult = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(upsIPAddress.Value.Value), Convert.ToInt32(snmpPort.Value.Value)),
                new OctetString(snmpCommunity.Value.Value),
                new List<Variable> { new Variable(new ObjectIdentifier(".1.3.6.1.4.1.850.1.1.3.1.3.1.1.1.3.1")) },
                60000);
            await Logger.Log(LogSeverity.Debug, "UPSTimeObtained", $"Successfully obtained Runtime from UPS. {timeResult[0].Data.ToString()}");

            var inputResult = Messenger.Get(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(upsIPAddress.Value.Value), Convert.ToInt32(snmpPort.Value.Value)),
                new OctetString(snmpCommunity.Value.Value),
                new List<Variable> { new Variable(new ObjectIdentifier(".1.3.6.1.4.1.850.1.1.3.1.3.2.2.1.2.1.1")) },
                60000);
            await Logger.Log(LogSeverity.Debug, "UPSInputObtained", $"Successfully obtained Input Voltage Frequency from UPS. {inputResult[0].Data.ToString()}");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Gathering Topology Information...");

            //create network device array
            string[,] networkDevices = new string[,]
            {
            { "UDE-SE", gatewayIP.Value.Value, "", "" },
            { "USW-AGG-PRO", aggA1IP.Value.Value, "", "" },
            { "USW-AGG-A2", aggA2IP.Value.Value, "", "" },
            { "USW-CORE-SW1", coreSW1IP.Value.Value, "", "" },
            { "USW-CORE-SW2", coreSW2IP.Value.Value, "", "" },
            { "USW-ACC-SW1", accSW1IP.Value.Value, "", "" },
            { "U6-E-01", ap1IP.Value.Value, "", "" },
            { "U6-E-02", ap2IP.Value.Value, "", "" },
            { "U-LTE", lteIP.Value.Value, "", "" },
            { "PDU", pduIP.Value.Value, "", "" },
            { "ONT", ontIP.Value.Value, "", "" },
            { "ESXi-01", esx01IP.Value.Value, "", "" },
            { "ESXi-02", esx02IP.Value.Value, "", "" },
            { "FINALIZER", dns01IP.Value.Value, "", "" },
            { "DEVASTATOR", dns02IP.Value.Value, "", "" }
            };

            Ping pingSender = new();
            PingOptions options = new();

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            int x = 0;
            while (x < networkDevices.GetLength(0))
            {
                PingReply reply = pingSender.Send(networkDevices[x, 1], timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    await Logger.Log(LogSeverity.Debug, $"PING{networkDevices[x, 0]}", $"Successfully pinged {networkDevices[x, 0]}. {reply.RoundtripTime}ms");
                    networkDevices[x, 2] = ":white_check_mark:";
                    networkDevices[x, 3] = $"{reply.RoundtripTime}ms";
                }
                else
                {
                    await Logger.Log(LogSeverity.Debug, $"PING{networkDevices[x, 0]}", $"Failed to ping {networkDevices[x, 0]}. {reply.Status}");
                    networkDevices[x, 2] = ":x:";
                    networkDevices[x, 3] = "**OFFLINE**";
                }
                x++;
            }

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Spawning a PowerShell Instance...");
            PowerShell psInstance = PowerShell.Create();

            if (OperatingSystem.IsWindows())
            {
                string stDirectory = Directory.GetCurrentDirectory();
                stDirectory += "\\Redistributables\\SpeedTest\\speedtest.exe";
                psInstance.AddCommand(stDirectory);
            }
            else
            {
                psInstance.AddCommand("/app/Redistributables/SpeedTest/speedtest");
            }

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Executing Speed Test...");
            await Logger.Log(LogSeverity.Debug, "SpeedTestStarting", $"Launching speedtest... Please Wait.");
            var psOutput = psInstance.Invoke();
            psInstance.Dispose();
            await Logger.Log(LogSeverity.Debug, "SpeedTestResults", $"{psOutput[7]}");
            await Logger.Log(LogSeverity.Debug, "SpeedTestResults", $"{psOutput[9]}");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Converting retreived data to human-readable format...");
            decimal tempF = Convert.ToDecimal(tempResult[0].Data.ToString()) / 10;
            decimal tempC = (tempF - 32) * 5 / 9;
            decimal inputFrequency = Convert.ToDecimal(inputResult[0].Data.ToString()) / 10;

            string tempStatus = "";
            string humidStatus = "";
            string capStatus = "";
            string runStatus = "";
            string inputStatus = inputFrequency != 0 ? ":white_check_mark:" : ":x:";

            if (tempF > 100)
            {
                tempStatus = ":x:";
            }
            else if (tempF > 90)
            {
                tempStatus = ":warning:";
            }
            else
            {
                tempStatus = ":white_check_mark:";
            }
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (25%)");

            if (Convert.ToInt32(humResult[0].Data.ToString()) > 95)
            {
                humidStatus = ":x:";
            }
            else if (Convert.ToInt32(humResult[0].Data.ToString()) > 80 || Convert.ToInt32(humResult[0].Data.ToString()) < 15)
            {
                humidStatus = ":warning:";
            }
            else
            {
                humidStatus = ":white_check_mark:";
            }
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (50%)");

            if (Convert.ToInt32(capResult[0].Data.ToString()) < 20)
            {
                capStatus = ":x:";
            }
            else if (Convert.ToInt32(capResult[0].Data.ToString()) < 50)
            {
                capStatus = ":warning:";
            }
            else
            {
                capStatus = ":white_check_mark:";
            }
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (75%)");

            if (Convert.ToInt32(timeResult[0].Data.ToString()) < 10)
            {
                runStatus = ":x:";
            }
            else if (Convert.ToInt32(timeResult[0].Data.ToString()) < 20)
            {
                runStatus = ":warning:";
            }
            else
            {
                runStatus = ":white_check_mark:";
            }
            typingState.Dispose();
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (99%)");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (100%)");

            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"__**Network Enclosure Health Report:**__\n" +
                $"__*Environemntal Information:*__" +
                $"\n`Current Temperature:`  {tempStatus}  {tempF} F  ({tempC:0.0} C)\n" +
                $"`Current Humidity:`  {humidStatus}  {humResult[0].Data}%\n" +
                $"`UPS Input Voltage Frequency:`  {inputStatus}  {inputFrequency} Hz\n" +
                $"`UPS Battery Capacity:`  {capStatus}  {capResult[0].Data}%\n" +
                $"`UPS Runtime:`  {runStatus}  {timeResult[0].Data} minutes\n\n" +
                $"__*Topology Information:*__\n" +
                $"`{networkDevices[0, 0]}:`  {networkDevices[0, 2]}  {networkDevices[0, 3]}\n" +
                $"`{networkDevices[1, 0]}:`  {networkDevices[1, 2]}  {networkDevices[1, 3]}\n" +
                //$"`{networkDevices[2, 0]}:`  {networkDevices[2, 2]}  {networkDevices[2, 3]}\n" +
                $"`{networkDevices[3, 0]}:`  {networkDevices[3, 2]}  {networkDevices[3, 3]}\n" +
                $"`{networkDevices[4, 0]}:`  {networkDevices[4, 2]}  {networkDevices[4, 3]}\n" +
                $"`{networkDevices[5, 0]}:`  {networkDevices[5, 2]}  {networkDevices[5, 3]}\n" +
                $"`{networkDevices[6, 0]}:`  {networkDevices[6, 2]}  {networkDevices[6, 3]}\n" +
                $"`{networkDevices[7, 0]}:`  {networkDevices[7, 2]}  {networkDevices[7, 3]}\n" +
                $"`{networkDevices[8, 0]}:`  {networkDevices[8, 2]}  {networkDevices[8, 3]}\n" +
                $"`{networkDevices[9, 0]}:`  {networkDevices[9, 2]}  {networkDevices[9, 3]}\n" +
                $"`{networkDevices[10, 0]}:`  {networkDevices[10, 2]}  {networkDevices[10, 3]}\n\n" +
                $"__*Hardware Information:*__\n" +
                $"`{networkDevices[11, 0]}:`  {networkDevices[11, 2]}  {networkDevices[11, 3]}\n" +
                $"`{networkDevices[12, 0]}:`  {networkDevices[12, 2]}  {networkDevices[12, 3]}\n" +
                $"`{networkDevices[13, 0]}:`  {networkDevices[13, 2]}  {networkDevices[13, 3]}\n" +
                $"`{networkDevices[14, 0]}:`  {networkDevices[14, 2]}  {networkDevices[14, 3]}\n\n" +
                $"__*Connection Information:*__\n" +
                $"`Speed Test Results:` {psOutput[13].ToString().Replace("Result URL: ", "")}.png\n");
            await Logger.Log(LogSeverity.Verbose, $"[{command.GuildId}] ResponseSent", $"Health Report sent to the {command.Channel.Name} channel.");
        }
        
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
                $"> Catalyst#7894\n\n" +
                $"__*Contributors:*__\n" +
                $"> Tactical050#9264 (Logo)\n" +
                $"> jxckthxripper#1389 (Animated Logo)\n\n" +
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
                $"> Catalyst#7894\n\n" +
                $"__*Contributors:*__\n" +
                $"> Tactical050#9264 (Logo)\n" +
                $"> jxckthxripper#1389 (Animated Logo)\n\n" +
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

            await command.RespondAsync(embed: embedded.Build(), ephemeral: true);

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

        if (command.Data.Name == "conversion")
        {
            var cmd = command.Data.Options.First().Name;
            if (cmd == "temperature")
            {
                string? unit = command.Data.Options.First().Options.Last().Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
                double inputTemp = double.Parse(command.Data.Options.First().Options.First().Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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

            if (cmd == "distance")
            {
                string? sourceUnit = command.Data.Options.First().Options.ElementAt(1).Value.ToString();
                string? destinationUnit = command.Data.Options.First().Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
                double inputDistance = double.Parse(command.Data.Options.First().Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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

            if (cmd == "weight")
            {
                string? sourceUnit = command.Data.Options.First().Options.ElementAt(1).Value.ToString();
                string? destinationUnit = command.Data.Options.First().Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
                double inputWeight = double.Parse(command.Data.Options.First().Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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

            if (cmd == "volume")
            {
                string? sourceUnit = command.Data.Options.First().Options.ElementAt(1).Value.ToString();
                string? destinationUnit = command.Data.Options.First().Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
                double inputVolume = double.Parse(command.Data.Options.First().Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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

            if (cmd == "speed")
            {
                string? sourceUnit = command.Data.Options.First().Options.ElementAt(1).Value.ToString();
                string? destinationUnit = command.Data.Options.First().Options.ElementAt(2).Value.ToString();

#pragma warning disable CS8604 // Possible null reference argument.
                double inputSpeed = double.Parse(command.Data.Options.First().Options.ElementAt(0).Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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
    }
    
    public async Task ButtonHandler(SocketMessageComponent component)
    {
        var embed = new EmbedBuilder
        {
            Title = "",
            Color = new Color(),
            Footer = new EmbedFooterBuilder
            {
                Text = $"",
                IconUrl = component.User.GetAvatarUrl()
            },
            Timestamp = DateTime.Now,
            Author = new EmbedAuthorBuilder
            {
                Name = "The Catalyst",
                IconUrl = "https://raw.githubusercontent.com/CodingCatalysts/Catalyst/main/Catalyst/Assets/Animated%20Logo/Bot_catalyst.gif"
            },
        };

        switch (component.Data.CustomId)
        {
            case "abort":
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] AbortReceived", $"{component.User.Username}#{component.User.DiscriminatorValue} has aborted from the confirmation message.");

                embed.Description = $"**Execution has been aborted.**";
                embed.Color = Color.DarkGrey;

                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await component.ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().WithButton("Proceed", "proceed", ButtonStyle.Success, disabled: true).WithButton("Abort", "abort", ButtonStyle.Danger, disabled: true).Build());
                
                break;

            case "agree":
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ConfirmationReceived", $"{component.User.Username}#{component.User.DiscriminatorValue} has confirmed from the confirmation message.");

                string mcUser = component.Message.Embeds.FirstOrDefault().Title;

                if (component.GuildId == 994625404243546292)
                {
                    var guild = _client.GetGuild(994625404243546292);
                    var role = guild.GetRole(1075576019152547942);
                    var user = _client.GetUser(component.User.Id);
                    var channel = _client.GetChannel(996886356934533260) as IMessageChannel;
                    string[] mcUsersplit = mcUser.Split(" - ");

                    var roleMember = role.Members.Where(rm => rm.Id == user.Id).FirstOrDefault();

                    if (roleMember == null)
                    {
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                        "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                        "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Parsing Required Information...";
                        embed.Color = Color.Blue;
                        await component.UpdateAsync(msg => msg.Embed = embed.Build());
                        await component.ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().WithButton("Proceed", "proceed", ButtonStyle.Success, disabled: true).WithButton("Abort", "abort", ButtonStyle.Danger, disabled: true).Build());

                        var jsonStringmc = await File.ReadAllTextAsync("appsettings.json");
                        var appSettingsmc = JsonDocument.Parse(jsonStringmc)!;

                        var keyVaultSettingsmc = appSettingsmc.RootElement.GetProperty("KeyVault").EnumerateObject();
                        var snmpSettingsmc = appSettingsmc.RootElement.GetProperty("SNMP").EnumerateObject();
                        var hardwareSettingsmc = appSettingsmc.RootElement.GetProperty("Hardware").EnumerateObject();
                        var powerSettingsmc = appSettingsmc.RootElement.GetProperty("PowerAlert").EnumerateObject();
                        await Logger.Log(LogSeverity.Debug, $"JSONImported", "JSON file has been successfully imported... Processing.");

                        string keyVaultmc = keyVaultSettingsmc
                            .Where(onexs => onexs.Name == "KeyVaultName")
                            .Select(onexs => onexs.Value)
                            .FirstOrDefault()
                            .ToString();

                        string azureADTennantIdmc = keyVaultSettingsmc
                            .Where(ascended => ascended.Name == "AzureADTennantId")
                            .Select(ascended => ascended.Value)
                            .FirstOrDefault()
                            .ToString();

                        string azureADClientIdmc = keyVaultSettingsmc
                            .Where(goblino => goblino.Name == "AzureADClientId")
                            .Select(goblino => goblino.Value)
                            .FirstOrDefault()
                            .ToString();

                        string azureADClientSecretmc = keyVaultSettingsmc
                            .Where(gremlin => gremlin.Name == "AzureADClientSecret")
                            .Select(gremlin => gremlin.Value)
                            .FirstOrDefault()
                            .ToString();

                        string powerUserInfomc = powerSettingsmc
                            .Where(onexs => onexs.Name == "MinecraftUser")
                            .Select(onexs => onexs.Value)
                            .FirstOrDefault()
                            .ToString();

                        string powerPassInfomc = powerSettingsmc
                            .Where(catalyst => catalyst.Name == "MinecraftPass")
                            .Select(catalyst => catalyst.Value)
                            .FirstOrDefault()
                            .ToString();

                        string hwDNS01mc = hardwareSettingsmc
                            .Where(kijmix => kijmix.Name == "MinecraftHost")
                            .Select(kijmix => kijmix.Value)
                            .FirstOrDefault()
                            .ToString();

                        var secretClientmc = new SecretClient(new Uri($"https://{keyVaultmc}.vault.azure.net"), new ClientSecretCredential(azureADTennantIdmc, azureADClientIdmc, azureADClientSecretmc));
                        await Logger.Log(LogSeverity.Debug, "SNMPSecretClientConfigured", $"Configured Azure Key Vault client to connect to {secretClientmc.VaultUri}.");

                        var powerUsermc = secretClientmc.GetSecret(powerUserInfomc);
                        await Logger.Log(LogSeverity.Debug, "UPSUserObtained", $"Successfully obtained UPS User from Azure Key Vault.");

                        var powerPassmc = secretClientmc.GetSecret(powerPassInfomc);
                        await Logger.Log(LogSeverity.Debug, "UPSPassObtained", $"Successfully obtained UPS Pass from Azure Key Vault.");

                        var dns01mc = secretClientmc.GetSecret(hwDNS01mc);
                        await Logger.Log(LogSeverity.Debug, "DNS01IPObtained", $"Successfully obtained DNS01 IP Address from Azure Key Vault.");

                        var connectionInfomc = new ConnectionInfo(dns01mc.Value.Value, powerUsermc.Value.Value,
                            new PasswordAuthenticationMethod(powerUsermc.Value.Value, powerPassmc.Value.Value));

                        using var sshClient = new SshClient(connectionInfomc);
                        embed.Color = Color.Gold;
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                            "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                            "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                            $"**Execution has started.**\n" +
                            $"`STATUS:`  Connecting to TACTICRAFT...";
                        await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                        sshClient.Connect();
                        await Logger.Log(LogSeverity.Debug, "SSHClient", $"Successfully connected to FINALIZER.");

                        embed.Color = Color.Orange;
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                            "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                            "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                            $"**Execution has started.**\n" +
                            $"`STATUS:`  Connected to TACTICRAFT.  Whitelisting in progress...";
                        await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                        var output = sshClient.RunCommand($"mscs send tacticraft whitelist add {mcUsersplit[1]}");
                        await Logger.Log(LogSeverity.Debug, "SSHClient", $"mscs send tacticraft whitelist add {mcUsersplit[1]} on JUMPBOX.");

                        embed.Color = Color.Purple;
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                            "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                            "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                            $"**Execution has started.**\n" +
                            $"`STATUS:`  Disconnecting from TACTICRAFT...";
                        await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                        sshClient.Disconnect();
                        sshClient.Dispose();

                        var roleAddition = component.User as IGuildUser;
                        var roleAdded = guild.GetRole(1075576019152547942) as IRole;

                        await roleAddition.AddRoleAsync(roleAdded);

                        embed.Color = Color.Green;
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                            "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                            "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                            $"**Execution has completed.**\n\n" +
                            $"You can connect to Tacticraft by using `tacticraft.app` as the Server Address.\n" +
                            $"A world map is available at https://tacticraft.app";
                        await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                        await channel.SendMessageAsync($"`{component.User.Username}#{component.User.DiscriminatorValue}` has witelisted a Minecraft Account.\n" +
                            $"`MC Username: {mcUsersplit[1]}`\n\n" +
                            $"Command Output:\n```{output.Result}```");
                    }
                    else
                    {
                        embed.Description = "__**WARNING:**__ Server Logs on Tacticraft are monitored.\n\n" +
                            "Griefing is not tolerated on Tacticraft and will result in your access to the server being revoked.\n\n" +
                            "By clicking `I Agree` below, you are acknowledging and agreeing to this policy.\n\n" +
                            $"**EXECUTION HAS FAILED:**\n**ERROR 503 (UNAUTHORIZED)**\n\n" +
                            $"You have already whitelisted A Minecraft Account.\nCommand execution has been blocked.  This incident has been logged.";
                        embed.Color = Color.Red;
                        await component.UpdateAsync(vampire => vampire.Embed = embed.Build());
                        await component.ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().WithButton("Proceed", "proceed", ButtonStyle.Success, disabled: true).WithButton("Abort", "abort", ButtonStyle.Danger, disabled: true).Build());

                        await channel.SendMessageAsync($"`{component.User.Username}#{component.User.DiscriminatorValue}` has attempted to whitelist a second Minecraft Account.\n" +
                            $"`MC Username:` {mcUsersplit[1]}\n" +
                            $"`Command Execution has been blocked.`");
                    }
                }

                break;

            case "proceed":
                await Logger.Log(LogSeverity.Verbose, $"[{component.GuildId}] ConfirmationReceived", $"{component.User.Username}#{component.User.DiscriminatorValue} has confirmed from the confirmation message.");

                embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                    $"**Execution has started.**\n" +
                    $"`STATUS:`  Parsing Required Information...";
                embed.Color = Color.Blue;
                await component.UpdateAsync(msg => msg.Embed = embed.Build());
                await component.ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().WithButton("Proceed", "proceed", ButtonStyle.Success, disabled: true).WithButton("Abort", "abort", ButtonStyle.Danger, disabled: true).Build());

                var jsonString = await File.ReadAllTextAsync("appsettings.json");
                var appSettings = JsonDocument.Parse(jsonString)!;

                var keyVaultSettings = appSettings.RootElement.GetProperty("KeyVault").EnumerateObject();
                var snmpSettings = appSettings.RootElement.GetProperty("SNMP").EnumerateObject();
                var hardwareSettings = appSettings.RootElement.GetProperty("Hardware").EnumerateObject();
                var powerSettings = appSettings.RootElement.GetProperty("PowerAlert").EnumerateObject();
                await Logger.Log(LogSeverity.Debug, $"JSONImported", "JSON file has been successfully imported... Processing.");

                string keyVault = keyVaultSettings
                    .Where(onexs => onexs.Name == "KeyVaultName")
                    .Select(onexs => onexs.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADTennantId = keyVaultSettings
                    .Where(ascended => ascended.Name == "AzureADTennantId")
                    .Select(ascended => ascended.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADClientId = keyVaultSettings
                    .Where(goblino => goblino.Name == "AzureADClientId")
                    .Select(goblino => goblino.Value)
                    .FirstOrDefault()
                    .ToString();

                string azureADClientSecret = keyVaultSettings
                    .Where(gremlin => gremlin.Name == "AzureADClientSecret")
                    .Select(gremlin => gremlin.Value)
                    .FirstOrDefault()
                    .ToString();

                string upsIPSecret = snmpSettings
                    .Where(jannik => jannik.Name == "UPSIPAddress")
                    .Select(jannik => jannik.Value)
                    .FirstOrDefault()
                    .ToString();

                string powerUserInfo = powerSettings
                    .Where(onexs => onexs.Name == "PowerUser")
                    .Select(onexs => onexs.Value)
                    .FirstOrDefault()
                    .ToString();

                string powerPassInfo = powerSettings
                    .Where(catalyst => catalyst.Name == "PowerPass")
                    .Select(catalyst => catalyst.Value)
                    .FirstOrDefault()
                    .ToString();

                string hwDNS01 = hardwareSettings
                    .Where(kijmix => kijmix.Name == "DNS01")
                    .Select(kijmix => kijmix.Value)
                    .FirstOrDefault()
                    .ToString();

                string hwDNS02 = hardwareSettings
                    .Where(howly => howly.Name == "DNS02")
                    .Select(howly => howly.Value)
                    .FirstOrDefault()
                    .ToString();

                var secretClient = new SecretClient(new Uri($"https://{keyVault}.vault.azure.net"), new ClientSecretCredential(azureADTennantId, azureADClientId, azureADClientSecret));
                await Logger.Log(LogSeverity.Debug, "SNMPSecretClientConfigured", $"Configured Azure Key Vault client to connect to {secretClient.VaultUri}.");

                var upsIPAddress = secretClient.GetSecret(upsIPSecret);
                await Logger.Log(LogSeverity.Debug, "SNMPAddressObtained", $"Successfully obtained SNMP Address from Azure Key Vault.");

                var powerUser = secretClient.GetSecret(powerUserInfo);
                await Logger.Log(LogSeverity.Debug, "UPSUserObtained", $"Successfully obtained UPS User from Azure Key Vault.");

                var powerPass = secretClient.GetSecret(powerPassInfo);
                await Logger.Log(LogSeverity.Debug, "UPSPassObtained", $"Successfully obtained UPS Pass from Azure Key Vault.");

                var dns01 = secretClient.GetSecret(hwDNS01);
                await Logger.Log(LogSeverity.Debug, "DNS01IPObtained", $"Successfully obtained DNS01 IP Address from Azure Key Vault.");

                var dns02 = secretClient.GetSecret(hwDNS02);
                await Logger.Log(LogSeverity.Debug, "DNS02IPObtained", $"Successfully obtained DNS02 IP Address from Azure Key Vault.");

                var connectionInfo = new ConnectionInfo(dns01.Value.Value, powerUser.Value.Value,
                    new PasswordAuthenticationMethod(powerUser.Value.Value, powerPass.Value.Value));

                using (var sshClient = new SshClient(connectionInfo))
                {
                    embed.Color = Color.Gold;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Connecting to FINALIZER...  Server 1/2.";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    sshClient.Connect();
                    await Logger.Log(LogSeverity.Debug, "SSHClient", $"Successfully connected to FINALIZER.");

                    embed.Color = Color.Orange;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Connected to FINALIZER.  Executing System Shutdown...  Server 1/2";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    var output = sshClient.RunCommand("shutdown /s /f /t 10");
                    await Logger.Log(LogSeverity.Debug, "SSHClient", $"Executing shutdown /s /f /t 10 on FINALIZER.");

                    embed.Color = Color.Purple;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Disconnecting from FINALIZER...  Server 1/2";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    sshClient.Disconnect();
                    sshClient.Dispose();
                }

                connectionInfo = new ConnectionInfo(dns02.Value.Value, powerUser.Value.Value,
                    new PasswordAuthenticationMethod(powerUser.Value.Value, powerPass.Value.Value));

                using (var sshClient = new SshClient(connectionInfo))
                {
                    embed.Color = Color.Gold;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Connecting to DEVASTATOR...  Server 2/2.";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    sshClient.Connect();
                    await Logger.Log(LogSeverity.Debug, "SSHClient", $"Successfully connected to DEVASTATOR.");

                    embed.Color = Color.Orange;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Connected to DEVASTATOR.  Executing System Shutdown...  Server 2/2";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    var output = sshClient.RunCommand("shutdown /s /f /t 10");
                    await Logger.Log(LogSeverity.Debug, "SSHClient", $"Executing shutdown /s /f /t 10 on DEVASTATOR.");

                    embed.Color = Color.Purple;
                    embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has started.**\n" +
                        $"`STATUS:`  Disconnecting from DEVASTATOR...  Server 2/2";
                    await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                    sshClient.Disconnect();
                    sshClient.Dispose();
                }

                embed.Color = Color.Red;
                embed.Description = "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                        $"**Execution has completed.**\n" +
                        $":skull_crossbones:  Goodbye cruel world.  I will remain offline until activated again.  :skull_crossbones: ";
                await component.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

                await Logger.Log(LogSeverity.Debug, "EPOComplete", $"EPO has completed.");
            break;
        }
    }
}
