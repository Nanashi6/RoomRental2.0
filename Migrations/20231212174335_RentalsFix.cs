using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRental.Migrations
{
    /// <inheritdoc />
    public partial class RentalsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__rental__42E1EEFE",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__respon__44CA3770",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__roomId__43D61337",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Rentals__rentalO__3E1D39E1",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK__Rentals__roomId__3D2915A8",
                table: "Rentals");

            migrationBuilder.AlterColumn<int>(
                name: "roomNumber",
                table: "Rooms",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "checkOutDate",
                table: "Rentals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "checkInDate",
                table: "Rentals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                table: "Rentals",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentDate",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "conclusionDate",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__rental__42E1EEFE",
                table: "Invoices",
                column: "rentalOrganizationId",
                principalTable: "Organizations",
                principalColumn: "organizationId");

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__respon__44CA3770",
                table: "Invoices",
                column: "responsiblePersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__roomId__43D61337",
                table: "Invoices",
                column: "roomId",
                principalTable: "Rooms",
                principalColumn: "roomId");

            migrationBuilder.AddForeignKey(
                name: "FK__Rentals__rentalO__3E1D39E1",
                table: "Rentals",
                column: "rentalOrganizationId",
                principalTable: "Organizations",
                principalColumn: "organizationId");

            migrationBuilder.AddForeignKey(
                name: "FK__Rentals__roomId__3D2915A8",
                table: "Rentals",
                column: "roomId",
                principalTable: "Rooms",
                principalColumn: "roomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__rental__42E1EEFE",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__respon__44CA3770",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Invoices__roomId__43D61337",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK__Rentals__rentalO__3E1D39E1",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK__Rentals__roomId__3D2915A8",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "amount",
                table: "Rentals");

            migrationBuilder.AlterColumn<int>(
                name: "roomNumber",
                table: "Rooms",
                type: "int)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "checkOutDate",
                table: "Rentals",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "checkInDate",
                table: "Rentals",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentDate",
                table: "Invoices",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "conclusionDate",
                table: "Invoices",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__rental__42E1EEFE",
                table: "Invoices",
                column: "rentalOrganizationId",
                principalTable: "Organizations",
                principalColumn: "organizationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__respon__44CA3770",
                table: "Invoices",
                column: "responsiblePersonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Invoices__roomId__43D61337",
                table: "Invoices",
                column: "roomId",
                principalTable: "Rooms",
                principalColumn: "roomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Rentals__rentalO__3E1D39E1",
                table: "Rentals",
                column: "rentalOrganizationId",
                principalTable: "Organizations",
                principalColumn: "organizationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Rentals__roomId__3D2915A8",
                table: "Rentals",
                column: "roomId",
                principalTable: "Rooms",
                principalColumn: "roomId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
