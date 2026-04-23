using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_music.Migrations
{
    /// <inheritdoc />
    public partial class AddVipToSong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "IsVip",
                table: "songs",
                type: "bit",
                nullable: false,
                defaultValue: 0ul);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "songs");
        }
    }
}
