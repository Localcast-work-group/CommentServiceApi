using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AllowAnonymous : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAllowAnonymousComments",
                table: "VideoCourses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAllowAnonymousComments",
                table: "VideoCourses");
        }
    }
}
