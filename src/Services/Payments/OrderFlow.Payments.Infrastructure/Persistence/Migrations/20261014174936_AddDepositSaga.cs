using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "wallet_transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "reference_id",
                table: "wallet_transactions",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "wallet_transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "deposit_sagas",
                columns: table => new
                {
                    deposit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    amount = table.Column<decimal>(
                        type: "numeric(14,2)",
                        precision: 14,
                        scale: 2,
                        nullable: false
                    ),
                    currency = table.Column<string>(
                        type: "character(3)",
                        fixedLength: true,
                        maxLength: 3,
                        nullable: false
                    ),
                    card_brand = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    card_last4 = table.Column<string>(
                        type: "character(4)",
                        fixedLength: true,
                        maxLength: 4,
                        nullable: false
                    ),
                    authorization_code = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: false
                    ),
                    recipient_email = table.Column<string>(
                        type: "character varying(320)",
                        maxLength: 320,
                        nullable: true
                    ),
                    recipient_name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: true
                    ),
                    locale = table.Column<string>(
                        type: "character varying(5)",
                        maxLength: 5,
                        nullable: false
                    ),
                    state = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    failure_reason = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    started_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    steps = table.Column<string>(type: "jsonb", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deposit_sagas", x => x.deposit_id);
                }
            );

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    message_type = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    processed_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => new { x.message_id, x.consumer });
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_deposit_sagas_owner_id_started_at",
                table: "deposit_sagas",
                columns: new[] { "owner_id", "started_at" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "deposit_sagas");

            migrationBuilder.DropTable(name: "inbox_messages");

            migrationBuilder.DropColumn(name: "reason", table: "wallet_transactions");

            migrationBuilder.DropColumn(name: "reference_id", table: "wallet_transactions");

            migrationBuilder.DropColumn(name: "status", table: "wallet_transactions");
        }
    }
}
