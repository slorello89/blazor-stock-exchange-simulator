using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketMechanicsDemo.Server.Migrations
{
    public partial class InitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuyOrSell = table.Column<int>(nullable: false),
                    LimitOrMarket = table.Column<int>(nullable: false),
                    Symbol = table.Column<string>(nullable: true),
                    NumSharesRemaining = table.Column<int>(nullable: false),
                    NumSharesInital = table.Column<int>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    TimePlaced = table.Column<string>(nullable: true),
                    FillPrice = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
