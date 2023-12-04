using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mail.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    HasUnread = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<string>(type: "TEXT", nullable: true),
                    Attribute = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdMail = table.Column<string>(type: "TEXT", nullable: false),
                    ParentFolderId = table.Column<string>(type: "TEXT", nullable: false),
                    Unread = table.Column<bool>(type: "INTEGER", nullable: false),
                    From = table.Column<string>(type: "TEXT", nullable: true),
                    To = table.Column<string>(type: "TEXT", nullable: true),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    Html = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MailAdr = table.Column<string>(type: "TEXT", nullable: false),
                    HostSmtp = table.Column<string>(type: "TEXT", nullable: false),
                    PortSmtp = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUseAuthSmtp = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsernameSmtp = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordSmtp = table.Column<string>(type: "TEXT", nullable: true),
                    IsSslSmtp = table.Column<int>(type: "INTEGER", nullable: false),
                    HostImap = table.Column<string>(type: "TEXT", nullable: false),
                    PortImap = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUseAuthImap = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsernameImap = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordImap = table.Column<string>(type: "TEXT", nullable: true),
                    IsSslImap = table.Column<int>(type: "INTEGER", nullable: false),
                    HasUnread = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
