using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineRunCompletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PipelineRuns");
        }
    }
}
