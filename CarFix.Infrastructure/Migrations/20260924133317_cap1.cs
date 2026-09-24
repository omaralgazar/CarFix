using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class cap1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterSpecialties");

            migrationBuilder.CreateTable(
                name: "CenterCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VehicleBrand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterCapabilities_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalTable: "ServiceCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterCapabilities_ServiceCenterId_IssueCategory_VehicleBra~",
                table: "CenterCapabilities",
                columns: new[] { "ServiceCenterId", "IssueCategory", "VehicleBrand" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterCapabilities");

            migrationBuilder.CreateTable(
                name: "CenterSpecialties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterSpecialties_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalTable: "ServiceCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterSpecialties_ServiceCenterId_Type_Value",
                table: "CenterSpecialties",
                columns: new[] { "ServiceCenterId", "Type", "Value" },
                unique: true);
        }
    }
}
