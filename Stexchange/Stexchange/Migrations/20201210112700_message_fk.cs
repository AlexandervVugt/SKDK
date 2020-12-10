using Microsoft.EntityFrameworkCore.Migrations;

namespace Stexchange.Migrations
{
    public partial class message_fk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_id",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_chat_id",
                table: "Messages",
                column: "chat_id",
                principalTable: "Chats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_chat_id",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_id",
                table: "Messages",
                column: "id",
                principalTable: "Chats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
