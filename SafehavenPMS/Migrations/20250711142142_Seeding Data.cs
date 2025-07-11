using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class SeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EducationLevels",
                columns: new[] { "EducationLevelId", "EducationLevelName" },
                values: new object[,]
                {
                    { 1, "Primary" },
                    { 2, "Secondary" },
                    { 3, "Tertiary" },
                    { 4, "Postgraduate" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatuses",
                columns: new[] { "MaritalStatusId", "MaritalStatusType" },
                values: new object[,]
                {
                    { 1, "Single" },
                    { 2, "Married" },
                    { 3, "Divorced" },
                    { 4, "Widowed" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EducationLevels",
                keyColumn: "EducationLevelId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EducationLevels",
                keyColumn: "EducationLevelId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EducationLevels",
                keyColumn: "EducationLevelId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EducationLevels",
                keyColumn: "EducationLevelId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MaritalStatuses",
                keyColumn: "MaritalStatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaritalStatuses",
                keyColumn: "MaritalStatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaritalStatuses",
                keyColumn: "MaritalStatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaritalStatuses",
                keyColumn: "MaritalStatusId",
                keyValue: 4);
        }
    }
}
