using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShadowSession.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "programs",
                columns: table => new
                {
                    program_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    filename = table.Column<string>(type: "TEXT", nullable: false),
                    path = table.Column<string>(type: "TEXT", nullable: false),
                    automatic_recording_enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    recording_framerate = table.Column<int>(type: "INTEGER", nullable: true),
                    recording_bitrate = table.Column<int>(type: "INTEGER", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programs", x => x.program_id);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    user_setting_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    kind = table.Column<int>(type: "INTEGER", nullable: false),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    default_value = table.Column<string>(type: "TEXT", nullable: true),
                    value = table.Column<string>(type: "TEXT", nullable: true),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    is_value_required = table.Column<bool>(type: "INTEGER", nullable: false),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false),
                    visible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.user_setting_id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    program_id = table.Column<int>(type: "INTEGER", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_sessions_programs_program_id",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "program_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recordings",
                columns: table => new
                {
                    recording_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    session_id = table.Column<int>(type: "INTEGER", nullable: false),
                    file_path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordings", x => x.recording_id);
                    table.ForeignKey(
                        name: "FK_recordings_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordings_session_id",
                table: "recordings",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_program_id",
                table: "sessions",
                column: "program_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordings");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "programs");
        }
    }
}
