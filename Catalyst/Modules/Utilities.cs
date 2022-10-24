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

    //[Command("tic", RunMode = RunMode.Async)]
    //public async Task Tic()
    //{
    //    var whiteCheckMark = new Emoji("\u2705");
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

    //    await Context.Message.AddReactionAsync(whiteCheckMark);
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

    //    var typingState = Context.Channel.TriggerTypingAsync();
    //    var response = await Context.Message.ReplyAsync($"TAC");
    //    typingState.Dispose();
    //}

    //[Command("050", RunMode = RunMode.Async)]
    //public async Task ZeroFiveZero()
    //{
    //    var whiteCheckMark = new Emoji("\u2705");
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

    //    await Context.Message.AddReactionAsync(whiteCheckMark);
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

    //    var typingState = Context.Channel.TriggerTypingAsync();
    //    var response = await Context.Message.ReplyAsync($"(҂‾ ▵‾)︻デ═一 (˚▽˚’!)/");
    //    typingState.Dispose();
    //}
    
    //[Command("taccat", RunMode = RunMode.Async)]
    //public async Task TacCat()
    //{
    //    var whiteCheckMark = new Emoji("\u2705");
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

    //    await Context.Message.AddReactionAsync(whiteCheckMark);
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

    //    var typingState = Context.Channel.TriggerTypingAsync();
    //    var response = await Context.Message.ReplyAsync($"cattac");
    //    typingState.Dispose();
    //}
    
    //[Command("cattac", RunMode = RunMode.Async)]
    //public async Task CatTac()
    //{
    //    var whiteCheckMark = new Emoji("\u2705");
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

    //    await Context.Message.AddReactionAsync(whiteCheckMark);
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

    //    var typingState = Context.Channel.TriggerTypingAsync();
    //    var response = await Context.Message.ReplyAsync($"taccat");
    //    typingState.Dispose();
    //}

    //[Command("rower", RunMode = RunMode.Async)]
    //public async Task Rower()
    //{
    //    var whiteCheckMark = new Emoji("\u2705");
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandReceived", $"{Context.User.Username}#{Context.User.DiscriminatorValue} has invoked {Context.Message.Content} from the {Context.Channel.Name} channel.");

    //    await Context.Message.AddReactionAsync(whiteCheckMark);
    //    await Logger.Log(LogSeverity.Verbose, $"[{Context.Guild.Name}] CommandAcknowledged", $"Reacted with :white_check_mark: to {Context.User.Username}#{Context.User.DiscriminatorValue}'s message.");

    //    var typingState = Context.Channel.TriggerTypingAsync();
    //    var response = await Context.Message.ReplyAsync($"It's not a guillotine... it's a water rower.\n" +
    //        $"https://ergatta.com/the-ergatta-rower-v2/");
    //    typingState.Dispose();
    //}
}
