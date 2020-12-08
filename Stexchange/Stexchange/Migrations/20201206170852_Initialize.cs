﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace Stexchange.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Filters",
                columns: table => new
                {
                    value = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filters", x => x.value);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(254)", nullable: false),
                    username = table.Column<string>(type: "varchar(15)", nullable: false),
                    postal_code = table.Column<string>(type: "char(6)", nullable: false),
                    password = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    verified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                    table.UniqueConstraint("AK_Users_email", x => x.email);
                    table.UniqueConstraint("AK_Users_username", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(80)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    name_nl = table.Column<string>(type: "varchar(50)", nullable: false),
                    name_lt = table.Column<string>(type: "varchar(50)", nullable: true),
                    quantity = table.Column<int>(type: "int unsigned", nullable: false),
                    user_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    visible = table.Column<bool>(type: "bit(1)", nullable: false, defaultValue: true),
                    renewed = table.Column<bool>(type: "bit(1)", nullable: false, defaultValue: false),
                    last_modified = table.Column<DateTime>(type: "timestamp", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.id);
                    table.ForeignKey(
                        name: "FK_Listings_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVerifications",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "serial", nullable: false),
                    verification_code = table.Column<byte[]>(type: "varbinary(16)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerifications", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_UserVerifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ad_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    responder_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.id);
                    table.UniqueConstraint("AK_Chats_ad_id_responder_id", x => new { x.ad_id, x.responder_id });
                    table.ForeignKey(
                        name: "FK_Chats_Listings_ad_id",
                        column: x => x.ad_id,
                        principalTable: "Listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chats_Users_responder_id",
                        column: x => x.responder_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterListings",
                columns: table => new
                {
                    listing_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    filter_value = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterListings", x => new { x.listing_id, x.filter_value });
                    table.ForeignKey(
                        name: "FK_FilterListings_Listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "Listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterListings_Filters_filter_value",
                        column: x => x.filter_value,
                        principalTable: "Filters",
                        principalColumn: "value",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    listing_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    image = table.Column<byte[]>(type: "LONGBLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.id);
                    table.ForeignKey(
                        name: "FK_Images_Listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "Listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    chat_id = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    content = table.Column<string>(type: "varchar(1024)", nullable: false),
                    sender = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_id",
                        column: x => x.id,
                        principalTable: "Chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_sender",
                        column: x => x.sender,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_responder_id",
                table: "Chats",
                column: "responder_id");

            migrationBuilder.CreateIndex(
                name: "IX_FilterListings_filter_value",
                table: "FilterListings",
                column: "filter_value");

            migrationBuilder.CreateIndex(
                name: "IX_Images_listing_id",
                table: "Images",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_user_id",
                table: "Listings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_sender",
                table: "Messages",
                column: "sender");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerifications_verification_code",
                table: "UserVerifications",
                column: "verification_code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterListings");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "UserVerifications");

            migrationBuilder.DropTable(
                name: "Filters");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
