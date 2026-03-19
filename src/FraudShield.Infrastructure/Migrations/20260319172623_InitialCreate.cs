using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudShield.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_Document = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer_Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer_IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer_DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merchant_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Merchant_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merchant_Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merchant_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merchant_State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merchant_City = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions");
        }
    }
}
