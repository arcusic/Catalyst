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
using Catalyst.Common;
using Renci.SshNet;
using RunMode = Discord.Commands.RunMode;
using System.Management.Automation;

namespace Catalyst.Modules;

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

        var typingState = Context.Channel.EnterTypingState();

        var response = await Context.Message.ReplyAsync($"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Preparing for execution...");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"'Executing infrastructure health check... please wait.' in the {Context.Channel.Name} channel.");
        await Context.Channel.TriggerTypingAsync();

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Parsing required information...");
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
            .Where(nova => nova.Name == "AggA1")
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

        await Logger.Log(LogSeverity.Debug, $"JSONParsed", "JSON file has been successfully parsed.");

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Retreiving Secrets from Azure KeyVault...");
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

        var dns01IP = secretClient.GetSecret(hwDNS01);
        await Logger.Log(LogSeverity.Debug, "DNS01IPObtained", $"Successfully obtained DNS01 IP Address from Azure Key Vault.");

        var dns02IP = secretClient.GetSecret(hwDNS02);
        await Logger.Log(LogSeverity.Debug, "DNS02IPObtained", $"Successfully obtained DNS02 IP Address from Azure Key Vault.");

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Gathering Environmental Information...");
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

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Gathering Topology Information...");

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
            { "U-LTE", lteIP.Value.Value, "", "" },
            { "FINALIZER", dns01IP.Value.Value, "", "" },
            { "DEVASTATOR", dns02IP.Value.Value, "", "" },
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

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Spawning a PowerShell Instance...");
        PowerShell psInstance = PowerShell.Create();

        if (OperatingSystem.IsWindows())
        {
            string stDirectory = Directory.GetCurrentDirectory();
            stDirectory += "\\Redistributables\\SpeedTest\\speedtest.exe";
            psInstance.AddCommand(stDirectory);
        }
        else
        {
            psInstance.AddCommand("speedtest");
        }

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Executing Speed Test...");
        await Logger.Log(LogSeverity.Debug, "SpeedTestStarting", $"Launching speedtest.exe... Please Wait.");
        var psOutput = psInstance.Invoke();
        psInstance.Dispose();
        await Logger.Log(LogSeverity.Debug, "SpeedTestResults", $"{psOutput[7]}");
        await Logger.Log(LogSeverity.Debug, "SpeedTestResults", $"{psOutput[9]}");

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Converting retreived data to human-readable format...");
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
        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (25%)");

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
        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (50%)");

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
        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (75%)");

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
        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (99%)");

        await response.ModifyAsync(msg => msg.Content = $"Executing infrastructure health check... please wait.\n\n`CURRENT STATUS:`  Analyzing Environmental Health... (100%)");
        
        await response.ModifyAsync(msg => msg.Content = $"__**Network Enclosure Health Report:**__\n" +
            $"__*Environemntal Information:*__" +
            $"\n`Current Temperature:`  {tempStatus}  {tempF} F  ({tempC:0.0} C)\n" +
            $"`Current Humidity:`  {humidStatus}  {humResult[0].Data}%\n" +
            $"`UPS Input Voltage Frequency:`  {inputStatus}  {inputFrequency} Hz\n" +
            $"`UPS Battery Capacity:`  {capStatus}  {capResult[0].Data}%\n" +
            $"`UPS Runtime:`  {runStatus}  {timeResult[0].Data} minutes\n\n" +
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
            $"`{networkDevices[9, 0]}:`  {networkDevices[9, 2]}  {networkDevices[9, 3]}\n" +
            $"`{networkDevices[10, 0]}:`  {networkDevices[10, 2]}  {networkDevices[10, 3]}\n\n" +
            $"__*Connection Information:*__\n" +
            $"`Speed Test Results:` {psOutput[11].ToString().Replace("Result URL: ", "")}.png\n");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ResponseSent", $"Health Report sent to the {Context.Channel.Name} channel.");
    }
    [Command("epo", RunMode = RunMode.Async)]
    public async Task EmergencyPowerOff()
    {
        var whiteCheckMark = new Emoji("\u2705");
        var redX = new Emoji("\u274C");
        var denied = new Emoji("\uD83D\uDEAB");

        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");
        
        if (Context.User.Username == "Catalyst" && Context.User.DiscriminatorValue == 7894)
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Message.AddReactionAsync(whiteCheckMark);
            await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

            var response = await Context.Channel.SendMessageAsync("***Emergency Power Off***\n" +
            "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
            $"`Abort Execution by reacting with` {redX}\n " +
            $"`Confirm Execution by reacting with` {whiteCheckMark}\n");

            await response.AddReactionAsync(redX);
            await response.AddReactionAsync(whiteCheckMark);
            await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ConfirmationRequired", $"Confirmation Message posted... awaiting user input.");

            bool reacted = false;

            while ((!reacted))
            {
                
                var message = await Context.Channel.GetMessageAsync(response.Id);
                var confirm = await message.GetReactionUsersAsync(whiteCheckMark, 100).FlattenAsync();
                var abort = await message.GetReactionUsersAsync(redX, 100).FlattenAsync();

                foreach (var user in confirm)
                {
                    if (user.Username == "Catalyst" && user.Discriminator == "7894")
                    {
                        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ConfirmationReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has reacted with :white_check_mark: to the confirmation message.");
                        await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                            "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                            $"**Execution has started.**\n" +
                            $"`STATUS:`  Parsing Required Information...");

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
                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Connecting to FINALIZER...  Server 1/2.");

                            sshClient.Connect();
                            await Logger.Log(LogSeverity.Debug, "SSHClient", $"Successfully connected to FINALIZER.");

                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Connected to FINALIZER.  Executing System Shutdown...  Server 1/2");

                            var output = sshClient.RunCommand("shutdown /s /f /t 10");
                            await Logger.Log(LogSeverity.Debug, "SSHClient", $"Executing shutdown /s /f /t 10 on FINALIZER.");

                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Disconnecting from FINALIZER...  Server 1/2");

                            sshClient.Disconnect();
                            sshClient.Dispose();
                        }

                        connectionInfo = new ConnectionInfo(dns02.Value.Value, powerUser.Value.Value,
                            new PasswordAuthenticationMethod(powerUser.Value.Value, powerPass.Value.Value));

                        using (var sshClient = new SshClient(connectionInfo))
                        {
                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Connecting to DEVASTATOR...  Server 2/2.");

                            sshClient.Connect();
                            await Logger.Log(LogSeverity.Debug, "SSHClient", $"Successfully connected to DEVASTATOR.");

                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Connected to DEVASTATOR.  Executing System Shutdown...  Server 2/2");

                            var output = sshClient.RunCommand("shutdown /s /f /t 10");
                            await Logger.Log(LogSeverity.Debug, "SSHClient", $"Executing shutdown /s /f /t 10 on DEVASTATOR.");

                            await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has started.**\n" +
                                $"`STATUS:`  Disconnecting from DEVASTATOR...  Server 2/2");

                            sshClient.Disconnect();
                            sshClient.Dispose();
                        }

                        await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                                "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                                $"**Execution has completed.**\n" +
                                $":skull_crossbones:  Goodbye cruel world.  I will remain offline until activated again.  :skull_crossbones: ");
                        await Logger.Log(LogSeverity.Debug, "EPOComplete", $"EPO has completed.");

                        reacted = true;
                    }
                    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] ConfirmationRequired", $"No reaction found... awaiting user input.");
                }

                foreach (var user in abort)
                {
                    if (user.Username == "Catalyst" && user.Discriminator == "7894")
                    {
                        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] AbortReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has reacted with :x: to the confirmation message.");
                        await response.ModifyAsync(msg => msg.Content = "***Emergency Power Off***\n" +
                            "__**WARNING:**__ This command will `Shut Down` the Servers within the Enclosure!\n\n" +
                            $"**Execution has been aborted.**");

                        reacted = true;
                    }
                }
            }

        }
        else
        {
            await Context.Message.AddReactionAsync(denied);
            await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandRejected", $"Reacted with :no_entry_sign: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

            await Context.Channel.TriggerTypingAsync();

            await Context.Message.ReplyAsync(":no_entry:  ***UNAUTHORIZED***  :no_entry:\n" +
                "You have attempted to execute a privledged command without propper permissions.\n\n" +
                "__**WARNING:**__  This incident has been logged!\n" +
                "*Further attempts to execute a privledged command without authorization may lead to additional action.*");
        }
    }

    [Command("tic", RunMode = RunMode.Async)]
    public async Task Tic()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        var typingState = Context.Channel.TriggerTypingAsync();
        var response = await Context.Message.ReplyAsync($"TAC");
        typingState.Dispose();
    }

    [Command("050", RunMode = RunMode.Async)]
    public async Task ZeroFiveZero()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        var typingState = Context.Channel.TriggerTypingAsync();
        var response = await Context.Message.ReplyAsync($"(҂‾ ▵‾)︻デ═一 (˚▽˚’!)/");
        typingState.Dispose();
    }
    
    [Command("taccat", RunMode = RunMode.Async)]
    public async Task ZeroFiveZero()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        var typingState = Context.Channel.TriggerTypingAsync();
        var response = await Context.Message.ReplyAsync($"cattac");
        typingState.Dispose();
    }
    
    [Command("cattac", RunMode = RunMode.Async)]
    public async Task ZeroFiveZero()
    {
        var whiteCheckMark = new Emoji("\u2705");
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

        await Context.Message.AddReactionAsync(whiteCheckMark);
        await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

        var typingState = Context.Channel.TriggerTypingAsync();
        var response = await Context.Message.ReplyAsync($"taccat");
        typingState.Dispose();
    }
}
