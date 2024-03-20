using System;
using System.Collections.Generic;

namespace Catalyst.Models;

/// <summary>
/// Registered Discord Guilds
/// </summary>
public partial class TblServer
{
    public int ServerId { get; set; }

    public double DiscordGuildId { get; set; }

    public bool Inactive { get; set; }

    public DateTime DateAdded { get; set; }

    public DateTime DateUpdated { get; set; }

    public virtual ICollection<TblAccount> TblAccounts { get; set; } = new List<TblAccount>();
}
