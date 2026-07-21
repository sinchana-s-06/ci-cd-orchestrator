using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubRunMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GitHubRunId",
                table: "PipelineRuns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GitHubRunNumber",
                table: "PipelineRuns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubRunUrl",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubWorkflowName",
                table: "PipelineRuns",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubRunId",
                table: "PipelineRuns");

            migrationBuilder.DropColumn(
                name: "GitHubRunNumber",
                table: "PipelineRuns");

            migrationBuilder.DropColumn(
                name: "GitHubRunUrl",
                table: "PipelineRuns");

            migrationBuilder.DropColumn(
                name: "GitHubWorkflowName",
                table: "PipelineRuns");
        }
    }
}
