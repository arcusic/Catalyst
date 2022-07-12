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
        var topologySettings = appSettings.RootElement.GetProperty("Topology").EnumerateObject();
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

        string topGW = topologySettings
            .Where(kxunna => kxunna.Name == "GW")
            .Select(kxunna => kxunna.Value)
            .FirstOrDefault()
            .ToString();

        string topAGG1 = topologySettings
            .Where(device => device.Name == "AggA1")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();
        
        string topAGG2 = topologySettings
            .Where(device => device.Name == "AggA2")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();
        
        string topCore1 = topologySettings
            .Where(device => device.Name == "CoreSW1")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();

        string topCore2 = topologySettings
            .Where(device => device.Name == "CoreSW2")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();

        string topACC1 = topologySettings
            .Where(device => device.Name == "AccSW1")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();

        string topAP1 = topologySettings
            .Where(device => device.Name == "AP01")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();

        string topAP2 = topologySettings
            .Where(device => device.Name == "AP02")
            .Select(device => device.Value)
            .FirstOrDefault()
            .ToString();

        string topLTE = topologySettings
            .Where(device => device.Name == "LTE")
            .Select(device => device.Value)
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

        //create network device array
        string[,] networkDevices = new string[,]
        {
            { "UDE-SE", gatewayIP.Value.Value, "", "" },
            { "USW-AGG-A1", aggA1IP.Value.Value, "", "" },
            { "USW-AGG-A2", aggA2IP.Value.Value, "", "" },
            { "USW-CORE-SW1", coreSW1IP.Value.Value, "", "" },
            { "USW-CORE-SW2", coreSW2IP.Value.Value, "", "" },
            { "USW-ACC-SW1", accSW1IP.Value.Value, "", "" },
            { "U6-LR-01", ap1IP.Value.Value, "", "" },
            { "U6-LR-02", ap2IP.Value.Value, "", "" },
            { "U-LTE", lteIP.Value.Value, "", "" }
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

        var speed = await FastClient.GetDownloadSpeed(SpeedTest.Net.Enums.SpeedTestUnit.MegaBitsPerSecond);
        await Logger.Log(LogSeverity.Debug, "SpeedTestServer", $"SpeedTest has completed. Download Speed ({speed.Source}): {speed.Speed} {speed.Unit}");

        await Context.Message.ReplyAsync($"__**Network Enclosure Health Report:**__\n" +
            $"__*Environemntal Information:*__" +
            $"\n`Current Temperature:` {Convert.ToDecimal(tempResult[0].Data.ToString()) / 10} F\n" +
            $"`Current Humidity:` {humResult[0].Data}%\n" +
            $"`UPS Battery Capacity:` {capResult[0].Data}%\n" +
            $"`UPS Runtime:` {timeResult[0].Data} minutes\n\n" +
            $"__*Topology Information:*__\n" +
            $"`{networkDevices[0,0]}:`  {networkDevices[0,2]}  {networkDevices[0,3]}\n" +
            $"`{networkDevices[1,0]}:`  {networkDevices[1,2]}  {networkDevices[1,3]}\n" +
            $"`{networkDevices[2,0]}:`  {networkDevices[2,2]}  {networkDevices[2,3]}\n" +
            $"`{networkDevices[3,0]}:`  {networkDevices[3,2]}  {networkDevices[3,3]}\n" +
            $"`{networkDevices[4,0]}:`  {networkDevices[4,2]}  {networkDevices[4,3]}\n" +
            $"`{networkDevices[5,0]}:`  {networkDevices[5,2]}  {networkDevices[5,3]}\n" +
            $"`{networkDevices[6,0]}:`  {networkDevices[6,2]}  {networkDevices[6,3]}\n" +
            $"`{networkDevices[7,0]}:`  {networkDevices[7,2]}  {networkDevices[7,3]}\n" +
            $"`{networkDevices[8,0]}:`  {networkDevices[8,2]}  {networkDevices[8,3]}\n\n" +
            $"__*Hardware Information:*__\n" +
            $"***Not Implemented***\n\n" +
            $"__*Connection Information:*__\n" +
            $"`ISP Connection Speed:` {speed.Speed:0.00} {speed.Unit}\n");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"Health Report sent to the {Context.Channel.Name} channel.");
    }
}
