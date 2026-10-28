// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore;

public class TableSplittingSqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : TableSplittingTestBase(fixture, testOutputHelper)
{
    public override async Task Can_remove_optional_dependent_with_json_complex_property()
    {
        await base.Can_remove_optional_dependent_with_json_complex_property();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL

UPDATE "Cars" SET "Specs" = @p0, "Color" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "c1"."c"
FROM "Cars" AS "c"
LEFT JOIN (
    SELECT "c0"."Id", "c0"."Color", "c0"."Specs" AS "c"
    FROM "Cars" AS "c0"
    WHERE "c0"."Color" IS NOT NULL
) AS "c1" ON "c"."Id" = CASE
    WHEN "c1"."Color" IS NOT NULL THEN "c1"."Id"
END
ORDER BY "c"."Id"
""");
    }

    public override async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await base.ExecuteUpdate_works_for_table_sharing(async);

        AssertSql(
            """
@p='1'

UPDATE "Vehicles" AS "v"
SET "SeatingCapacity" = @p
""",
            //
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Vehicles" AS "v"
    WHERE "v"."SeatingCapacity" <> 1)
""");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
