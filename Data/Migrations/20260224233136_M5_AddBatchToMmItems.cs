using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class M5_AddBatchToMmItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "StockTransferItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "GoodsReceiptItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "GoodsIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_BatchId",
                table: "StockTransferItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptItems_BatchId",
                table: "GoodsReceiptItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssueItems_BatchId",
                table: "GoodsIssueItems",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsIssueItems_Batches_BatchId",
                table: "GoodsIssueItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptItems_Batches_BatchId",
                table: "GoodsReceiptItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_Batches_BatchId",
                table: "StockTransferItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsIssueItems_Batches_BatchId",
                table: "GoodsIssueItems");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptItems_Batches_BatchId",
                table: "GoodsReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_Batches_BatchId",
                table: "StockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferItems_BatchId",
                table: "StockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptItems_BatchId",
                table: "GoodsReceiptItems");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssueItems_BatchId",
                table: "GoodsIssueItems");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "GoodsReceiptItems");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "GoodsIssueItems");
        }
    }
}
