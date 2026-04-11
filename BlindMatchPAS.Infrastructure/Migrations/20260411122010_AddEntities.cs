using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlindMatchPAS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchArea",
                table: "ProjectProposals");

            migrationBuilder.AddColumn<int>(
                name: "ResearchAreaId",
                table: "ProjectProposals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MatchRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectProposalId = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    MatchedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchRecords_AspNetUsers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchRecords_ProjectProposals_ProjectProposalId",
                        column: x => x.ProjectProposalId,
                        principalTable: "ProjectProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAreas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_ProjectProposalId",
                table: "MatchRecords",
                column: "ProjectProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRecords_SupervisorId",
                table: "MatchRecords",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.DropTable(
                name: "MatchRecords");

            migrationBuilder.DropTable(
                name: "ResearchAreas");

            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.AddColumn<string>(
                name: "ResearchArea",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
