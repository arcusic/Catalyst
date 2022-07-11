using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Discord;
using Discord.Commands;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using LocationBot.Common;
using SpeedTest.Net;
using RunMode = Discord.Commands.RunMode;

namespace LocationBot.Modules;

public class Utilities : ModuleBase<ShardedCommandContext>
{
    public CommandService CommandService { get; set; }

    [Command("hc", RunMode = RunMode.Async)]
    public async Task HealthCheck()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        await Context.Message.ReplyAsync($"Executing infrastructure health check... please wait.");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"'Executing infrastructure health check... please wait.' in the {Context.Channel.Name} channel.");

        var jsonString = await File.ReadAllTextAsync("appsettings.json");
        var appSettings = JsonDocument.Parse(jsonString)!;
        var keyVaultSettings = appSettings.RootElement.GetProperty("KeyVault").EnumerateObject();
        var snmpSettings = appSettings.RootElement.GetProperty("SNMP").EnumerateObject();
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

        string upsSnmpPort =  snmpSettings
            .Where(ghxst => ghxst.Name == "UPSSNMPPort")
            .Select(ghxst => ghxst.Value)
            .FirstOrDefault()
            .ToString();

        await Logger.Log(LogSeverity.Debug, $"JSONParsed", "JSON file has been successfully parsed.");

        var secretClient = new SecretClient(new Uri($"https://{keyVault}.vault.azure.net"), new ClientSecretCredential(azureADTennantId, azureADClientId, azureADClientSecret));
        await Logger.Log(LogSeverity.Debug, "SNMPSecretClientConfigured", $"Configured Azure Key Vault client to connect to {secretClient.VaultUri}.");

        var upsIPAddress = secretClient.GetSecret(upsIPSecret);
        await Logger.Log(LogSeverity.Debug, "SNMPAddressObtained", $"Successfully obtained SNMP Address from Azure Key Vault.");

        var snmpCommunity = secretClient.GetSecret(upsSnmpCommunity);
        await Logger.Log(LogSeverity.Debug, "SNMPCommunityObtained", $"Successfully obtained SNMP Community from Azure Key Vault.");

        var snmpPort = secretClient.GetSecret(upsSnmpPort);
        await Logger.Log(LogSeverity.Debug, "SNMPPortObtained", $"Successfully obtained SNMP Port from Azure Key Vault.");

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

        Ping pingSender = new();
        PingOptions options = new();

        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        int timeout = 120;

        //PingReply reply = pingSender.Send("x.x.x.x", timeout, buffer, options);

        //if (reply.Status == IPStatus.Success)
        //{
        //    Console.WriteLine("Address: {0}", reply.Address.ToString());
        //    Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
        //}
        //else
        //{

        //}

        var speed = await FastClient.GetDownloadSpeed(SpeedTest.Net.Enums.SpeedTestUnit.MegaBitsPerSecond);
        await Logger.Log(LogSeverity.Debug, "SpeedTestServer", $"SpeedTest has completed. Download Speed ({speed.Source}): {speed.Speed} {speed.Unit}");

        await Context.Message.ReplyAsync($"__**Network Enclosure Health Report:**__\n" +
            $"__*Environemntal Information:*__" +
            $"\n`Current Temperature:` {Convert.ToDecimal(tempResult[0].Data.ToString()) / 10} F\n" +
            $"`Current Humidity:` {humResult[0].Data}%\n" +
            $"`UPS Battery Capacity:` {capResult[0].Data}%\n" +
            $"`UPS Runtime:` {timeResult[0].Data} minutes\n\n" +
            $"__*Topology Information:*__\n" +
            $"***Not Implemented***\n\n" +
            $"__*Hardware Information:*__\n" +
            $"***Not Implemented***\n\n" +
            $"__*Connection Information:*__\n" +
            $"`ISP Connection Speed:` {speed.Speed:0.00} {speed.Unit}\n");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"Health Report sent to the {Context.Channel.Name} channel.");
    }
}
