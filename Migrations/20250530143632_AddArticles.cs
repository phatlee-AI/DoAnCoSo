using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "Articles",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //         Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            //         ImageUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //         CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //         UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //         IsPublished = table.Column<bool>(type: "bit", nullable: false),
            //         AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //         Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //         MetaTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //         MetaDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            //         Tags = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Articles", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_Articles_AspNetUsers_AuthorId",
            //             column: x => x.AuthorId,
            //             principalTable: "AspNetUsers",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_Articles_AuthorId",
            //     table: "Articles",
            //     column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "Articles");
        }
    }
}
