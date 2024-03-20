using System;
using System.Collections.Generic;

namespace Catalyst.Models;

/// <summary>
/// Registered Discord Accounts
/// </summary>
public partial class TblAccount
{
    public int AccountId { get; set; }

    public int ServerId { get; set; }

    public double DiscordUserId { get; set; }

    public string? McaccountName { get; set; }

    public DateTime DateAdded { get; set; }

    public DateTime DateUpdated { get; set; }

    public virtual TblServer Server { get; set; } = null!;

    public virtual ICollection<TblBan> TblBans { get; set; } = new List<TblBan>();
}
