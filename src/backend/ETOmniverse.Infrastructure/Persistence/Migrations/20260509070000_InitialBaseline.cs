namespace ETOmniverse.Infrastructure.Persistence.Migrations;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

[DbContext(typeof(EtOmniverseDbContext))]
[Migration("20260509070000_InitialBaseline")]
public partial class InitialBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
