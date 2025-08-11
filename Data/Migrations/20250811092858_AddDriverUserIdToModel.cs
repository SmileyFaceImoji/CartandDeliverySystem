using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartandDeliverySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverUserIdToModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverUserId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverUserId",
                table: "Orders");
        }
    }
}
