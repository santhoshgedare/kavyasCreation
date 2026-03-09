using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns to OrderItems (skip if already exist from a partial prior run)
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderItems') AND name = 'ProductVariantId')
                    ALTER TABLE [OrderItems] ADD [ProductVariantId] uniqueidentifier NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderItems') AND name = 'VariantDescription')
                    ALTER TABLE [OrderItems] ADD [VariantDescription] nvarchar(max) NULL;
                """);

            // Nuclear cleanup: discover and drop ALL FK constraints referencing
            // any of the variant tables, then drop the tables themselves.
            // This handles every possible partial-migration state.
            migrationBuilder.Sql("""
                DECLARE @sql NVARCHAR(MAX) = N'';

                SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
                    + '.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                    + ' DROP CONSTRAINT ' + QUOTENAME(fk.name) + ';'
                FROM sys.foreign_keys fk
                WHERE fk.referenced_object_id IN (
                    OBJECT_ID('ProductVariantOptions'),
                    OBJECT_ID('ProductVariants'),
                    OBJECT_ID('CategoryVariantTypes')
                )
                OR fk.parent_object_id IN (
                    OBJECT_ID('ProductVariantOptions'),
                    OBJECT_ID('ProductVariants'),
                    OBJECT_ID('CategoryVariantTypes')
                );

                IF LEN(@sql) > 0 EXEC sp_executesql @sql;

                IF OBJECT_ID('ProductVariantOptions', 'U') IS NOT NULL DROP TABLE [ProductVariantOptions];
                IF OBJECT_ID('ProductVariants',       'U') IS NOT NULL DROP TABLE [ProductVariants];
                IF OBJECT_ID('CategoryVariantTypes',   'U') IS NOT NULL DROP TABLE [CategoryVariantTypes];
                """);

            // Recreate all variant tables cleanly
            migrationBuilder.CreateTable(
                name: "CategoryVariantTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryVariantTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryVariantTypes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariantOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryVariantTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantOptions_CategoryVariantTypes_CategoryVariantTypeId",
                        column: x => x.CategoryVariantTypeId,
                        principalTable: "CategoryVariantTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductVariantOptions_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryVariantTypes_CategoryId_SortOrder",
                table: "CategoryVariantTypes",
                columns: new[] { "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_CategoryVariantTypeId",
                table: "ProductVariantOptions",
                column: "CategoryVariantTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_ProductVariantId_CategoryVariantTypeId",
                table: "ProductVariantOptions",
                columns: new[] { "ProductVariantId", "CategoryVariantTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVariantOptions");

            migrationBuilder.DropTable(
                name: "CategoryVariantTypes");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantDescription",
                table: "OrderItems");
        }
    }
}
