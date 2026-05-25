using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLTToeic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizOptionText4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionText4",
                table: "QuizQuestionOptions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionText4",
                table: "QuizQuestionOptions");
        }
    }
}
