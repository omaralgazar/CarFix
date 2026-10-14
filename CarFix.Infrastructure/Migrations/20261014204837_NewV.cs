using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RotatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RotatedAt",
                table: "RefreshTokens");
        }
    }
}
