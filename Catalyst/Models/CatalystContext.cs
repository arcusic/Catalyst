using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Catalyst.Models;

public partial class CatalystContext : DbContext
{
    public CatalystContext()
    {
    }
    private readonly string _connectionString;

    public CatalystContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    public virtual DbSet<TblAccount> TblAccounts { get; set; }

    public virtual DbSet<TblBan> TblBans { get; set; }

    public virtual DbSet<TblServer> TblServers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId);

            entity
                .ToTable("tblAccounts", tb => tb.HasComment("Registered Discord Accounts"))
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("MSSQL_tblAccounts_History", "dbo");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("AccountID");
            entity.Property(e => e.DateAdded).HasColumnType("datetime");
            entity.Property(e => e.DateUpdated).HasColumnType("datetime");
            entity.Property(e => e.DiscordUserId).HasColumnName("DiscordUserID");
            entity.Property(e => e.McaccountName)
                .HasMaxLength(16)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MCAccountName");
            entity.Property(e => e.ServerId).HasColumnName("ServerID");

            entity.HasOne(d => d.Server).WithMany(p => p.TblAccounts)
                .HasForeignKey(d => d.ServerId)
                .HasConstraintName("FK_tblAccounts_tblServers");
        });

        modelBuilder.Entity<TblBan>(entity =>
        {
            entity.HasKey(e => e.BanId);

            entity
                .ToTable("tblBans", tb => tb.HasComment("Discord Bans"))
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("MSSQL_tblBans_History", "dbo");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BanId)
                .ValueGeneratedNever()
                .HasColumnName("BanID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.BanCreated).HasColumnType("datetime");
            entity.Property(e => e.BanExpiration).HasColumnType("datetime");
            entity.Property(e => e.DateAdded).HasColumnType("datetime");
            entity.Property(e => e.DateUpdated).HasColumnType("datetime");
            entity.Property(e => e.ServerId).HasColumnName("ServerID");

            entity.HasOne(d => d.Account).WithMany(p => p.TblBans)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_tblBans_tblAccounts");
        });

        modelBuilder.Entity<TblServer>(entity =>
        {
            entity.HasKey(e => e.ServerId);

            entity
                .ToTable("tblServers", tb => tb.HasComment("Registered Discord Guilds"))
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("MSSQL_tblServers_History", "dbo");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.ServerId)
                .ValueGeneratedNever()
                .HasColumnName("ServerID");
            entity.Property(e => e.DateAdded).HasColumnType("datetime");
            entity.Property(e => e.DateUpdated).HasColumnType("datetime");
            entity.Property(e => e.DiscordGuildId).HasColumnName("DiscordGuildID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
