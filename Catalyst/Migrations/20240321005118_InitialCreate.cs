using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalyst.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblServers",
                columns: table => new
                {
                    ServerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DiscordGuildID = table.Column<double>(type: "float", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    Inactive = table.Column<bool>(type: "bit", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateUpdated = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysEndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysStartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblServers", x => x.ServerID);
                },
                comment: "Registered Discord Guilds")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");

            migrationBuilder.CreateTable(
                name: "tblAccounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    ServerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DiscordUserID = table.Column<double>(type: "float", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    MCAccountName = table.Column<string>(type: "char(16)", unicode: false, fixedLength: true, maxLength: 16, nullable: true)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateUpdated = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysEndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysStartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAccounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_tblAccounts_tblServers",
                        column: x => x.ServerID,
                        principalTable: "tblServers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Registered Discord Accounts")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");

            migrationBuilder.CreateTable(
                name: "tblBans",
                columns: table => new
                {
                    BanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    ServerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    PermBan = table.Column<bool>(type: "bit", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    BanCreated = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    BanExpiration = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    DateUpdated = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysEndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime"),
                    SysStartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBans", x => x.BanID);
                    table.ForeignKey(
                        name: "FK_tblBans_tblAccounts",
                        column: x => x.AccountID,
                        principalTable: "tblAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Discord Bans")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_tblAccounts_ServerID",
                table: "tblAccounts",
                column: "ServerID");

            migrationBuilder.CreateIndex(
                name: "IX_tblBans_AccountID",
                table: "tblBans",
                column: "AccountID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblBans")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblBans_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");

            migrationBuilder.DropTable(
                name: "tblAccounts")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblAccounts_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");

            migrationBuilder.DropTable(
                name: "tblServers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MSSQL_tblServers_History")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "dbo")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysEndTime")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysStartTime");
        }
    }
}
