using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineStageStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildStatus",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeployStatus",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TestStatus",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildStatus",
                table: "PipelineRuns");

            migrationBuilder.DropColumn(
                name: "DeployStatus",
                table: "PipelineRuns");

            migrationBuilder.DropColumn(
                name: "TestStatus",
                table: "PipelineRuns");
        }
    }
}
