using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Angelus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AngelType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Color", table: "Characters");

            migrationBuilder.AddColumn<string>(
                name: "AngelType",
                table: "Characters",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AngelType", table: "Characters");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Characters",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");
        }
    }
}
