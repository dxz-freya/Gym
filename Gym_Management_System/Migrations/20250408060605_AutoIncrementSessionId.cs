using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymManagement.Migrations
{
    /// <inheritdoc />
    public partial class AutoIncrementSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1,
                columns: new[] { "Capacity", "RoomName" },
                values: new object[] { 44, "Recovery Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2,
                columns: new[] { "Capacity", "RoomName" },
                values: new object[] { 21, "Mobility Area" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 1, 35, "Endurance Zone" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 4,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 1, 32, "Functional Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 5,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 1, 47, "Dance Studio" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 6,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 40, "Endurance Zone" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 7,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 30, "Spin Studio" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 8,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 40, "Cardio Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 9,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 49, "Dance Studio" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 10,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 33, "Strength Studio" });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "BranchId", "Capacity", "IsAvailable", "RoomName" },
                values: new object[,]
                {
                    { 11, 3, 48, true, "Stretch Zone" },
                    { 12, 3, 20, true, "Crossfit Zone" },
                    { 13, 3, 36, true, "Cardio Room" },
                    { 14, 3, 32, true, "Strength Studio" },
                    { 15, 3, 26, true, "Functional Room" },
                    { 16, 4, 49, true, "HIIT Area" },
                    { 17, 4, 29, true, "Dance Studio" },
                    { 18, 4, 39, true, "Mobility Area" },
                    { 19, 4, 45, true, "Yoga Room" },
                    { 20, 4, 50, true, "Stretch Zone" },
                    { 21, 5, 41, true, "Cardio Room" },
                    { 22, 5, 24, true, "Weight Room" },
                    { 23, 5, 29, true, "Mobility Area" },
                    { 24, 5, 44, true, "Recovery Room" },
                    { 25, 5, 42, true, "Endurance Zone" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 25);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1,
                columns: new[] { "Capacity", "RoomName" },
                values: new object[] { 20, "Yoga Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2,
                columns: new[] { "Capacity", "RoomName" },
                values: new object[] { 25, "Cardio Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 30, "Weight Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 4,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 2, 18, "Crossfit Zone" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 5,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 3, 15, "Spin Studio" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 6,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 3, 20, "Dance Studio" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 7,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 4, 12, "HIIT Area" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 8,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 4, 16, "Pilates Room" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 9,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 5, 10, "Stretch Zone" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 10,
                columns: new[] { "BranchId", "Capacity", "RoomName" },
                values: new object[] { 5, 22, "Functional Room" });
        }
    }
}
