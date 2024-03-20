using System;
using System.Collections.Generic;

namespace Catalyst.Models;

/// <summary>
/// Discord Bans
/// </summary>
public partial class TblBan
{
    public int BanId { get; set; }

    public int ServerId { get; set; }

    public int AccountId { get; set; }

    public bool PermBan { get; set; }

    public DateTime BanCreated { get; set; }

    public DateTime BanExpiration { get; set; }

    public DateTime DateAdded { get; set; }

    public DateTime DateUpdated { get; set; }

    public virtual TblAccount Account { get; set; } = null!;
}
