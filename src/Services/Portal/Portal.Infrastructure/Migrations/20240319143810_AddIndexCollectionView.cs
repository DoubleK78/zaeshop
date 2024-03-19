using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexCollectionView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionView_CollectionId",
                table: "CollectionView");

            migrationBuilder.CreateIndex(
                name: "nci_msft_1_CollectionView_394301034398E39CFFB2DD574D69974B",
                table: "CollectionView",
                column: "CollectionId")
                .Annotation("SqlServer:Include", new[] { "Date", "View" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "nci_msft_1_CollectionView_394301034398E39CFFB2DD574D69974B",
                table: "CollectionView");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionView_CollectionId",
                table: "CollectionView",
                column: "CollectionId");
        }
    }
}
