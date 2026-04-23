using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_music.Migrations
{
    public partial class AddVipAndSystemPlaylistColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CHỈ THÊM 2 CỘT VÀO BẢNG PLAYLISTS
            migrationBuilder.AddColumn<ulong>(
                name: "IsSystemPlaylist",
                table: "playlists",
                type: "bit",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "IsVipOnly",
                table: "playlists",
                type: "bit",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // XÓA 2 CỘT NẾU ROLLBACK
            migrationBuilder.DropColumn(
                name: "IsSystemPlaylist",
                table: "playlists");

            migrationBuilder.DropColumn(
                name: "IsVipOnly",
                table: "playlists");
        }
    }
}