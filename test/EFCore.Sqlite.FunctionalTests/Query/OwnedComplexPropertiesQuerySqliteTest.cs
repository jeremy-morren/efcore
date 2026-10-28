// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedComplexPropertiesQuerySqliteTest(NonSharedFixture fixture) : OwnedComplexPropertiesQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Owned_reference()
    {
        await base.Owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_separate_table()
    {
        await base.Owned_reference_separate_table();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_with_json_complex_property()
    {
        await base.Owned_reference_with_json_complex_property();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Address"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_json()
    {
        await base.Owned_reference_json();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_collection()
    {
        await base.Owned_collection();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
ORDER BY "o"."Id", "o0"."OwnerId", "o0"."Name"
""");
    }

    public override async Task Owned_collection_json()
    {
        await base.Owned_collection_json();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."OwnedCollection"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Filter_on_complex_property_of_owned_reference()
    {
        await base.Filter_on_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Owned_Address_Country_Code" = 'DE'
LIMIT 2
""");
    }

    public override async Task Project_complex_property_of_owned_reference()
    {
        await base.Project_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Project_nested_complex_property_of_owned_reference()
    {
        await base.Project_nested_complex_property_of_owned_reference();

        AssertSql(
            """
SELECT "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""");
    }

    public override async Task Owned_reference_in_subquery()
    {
        await base.Owned_reference_in_subquery();

        AssertSql(
            """
@p='5'

SELECT "o0"."Id", "o0"."Owned_Name", "o0"."Owned_Address_City", "o0"."Owned_Address_Street", "o0"."Owned_Address_Country_Code", "o0"."Owned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
    FROM "Owners" AS "o"
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Id" > 1
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_with_json_complex_property_in_subquery()
    {
        await base.Owned_reference_with_json_complex_property_in_subquery();

        AssertSql(
            """
@p='5'

SELECT "o0"."Id", "o0"."Owned_Name", "o0"."Address"
FROM (
    SELECT "o"."Id", "o"."Owned_Name", "o"."Address"
    FROM "Owners" AS "o"
    ORDER BY "o"."Id"
    LIMIT @p
) AS "o0"
WHERE "o0"."Id" > 1
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_with_distinct()
    {
        await base.Owned_reference_with_distinct();

        AssertSql(
            """
SELECT "o0"."Id", "o0"."Name", "o0"."Owned_Name", "o0"."Owned_Address_City", "o0"."Owned_Address_Street", "o0"."Owned_Address_Country_Code", "o0"."Owned_Address_Country_Name"
FROM (
    SELECT DISTINCT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
    FROM "Owners" AS "o"
) AS "o0"
ORDER BY "o0"."Id"
""");
    }

    public override async Task Owned_reference_in_union()
    {
        await base.Owned_reference_in_union();

        AssertSql(
            """
SELECT "u"."Id", "u"."Name", "o1"."Id", "o1"."Owned_Name", "o1"."Owned_Address_City", "o1"."Owned_Address_Street", "o1"."Owned_Address_Country_Code", "o1"."Owned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name"
    FROM "Owners" AS "o"
    WHERE "o"."Id" = 1
    UNION
    SELECT "o0"."Id", "o0"."Name"
    FROM "Owners" AS "o0"
    WHERE "o0"."Id" = 2
) AS "u"
LEFT JOIN "Owners" AS "o1" ON "u"."Id" = "o1"."Id"
ORDER BY "u"."Id"
""");
    }

    public override async Task Update_complex_property_of_owned_reference()
    {
        await base.Update_complex_property_of_owned_reference();

        AssertSql(
            """
@p2='1'
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='Oz' (Nullable = false) (Size = 2)

UPDATE "Owners" SET "Owned_Address_City" = @p0, "Owned_Address_Country_Name" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Owned_reference_on_TPH_owner()
    {
        await base.Owned_reference_on_TPH_owner();

        AssertSql(
            """
SELECT "o"."Id", "o"."Discriminator", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name", "o"."DerivedOwned_Name", "o"."DerivedOwned_Address_City", "o"."DerivedOwned_Address_Street", "o"."DerivedOwned_Address_Country_Code", "o"."DerivedOwned_Address_Country_Name"
FROM "Owners" AS "o"
ORDER BY "o"."Id"
""",
            //
            """
SELECT "o"."DerivedOwned_Address_City"
FROM "Owners" AS "o"
WHERE "o"."Discriminator" = 'SpecialOwner'
""");
    }

    public override async Task Owned_reference_on_TPT_owner()
    {
        await base.Owned_reference_on_TPT_owner();

        AssertSql(
            """
SELECT "o"."Id", "o"."Name", CASE
    WHEN "s"."Id" IS NOT NULL THEN 'SpecialOwner'
END AS "Discriminator", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name", "s"."Id", "s"."DerivedOwned_Name", "s"."DerivedOwned_Address_City", "s"."DerivedOwned_Address_Street", "s"."DerivedOwned_Address_Country_Code", "s"."DerivedOwned_Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "SpecialOwner" AS "s" ON "o"."Id" = "s"."Id"
ORDER BY "o"."Id"
""",
            //
            """
SELECT "s"."DerivedOwned_Address_City"
FROM "Owners" AS "o"
LEFT JOIN "SpecialOwner" AS "s" ON "o"."Id" = "s"."Id"
WHERE "s"."Id" IS NOT NULL
""");
    }

    public override async Task Owned_reference_on_TPC_owner()
    {
        await base.Owned_reference_on_TPC_owner();

        AssertSql(
            """
SELECT "u"."Id", "u"."Name", "u"."Discriminator", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name", "s0"."Id", "s0"."DerivedOwned_Name", "s0"."DerivedOwned_Address_City", "s0"."DerivedOwned_Address_Street", "s0"."DerivedOwned_Address_Country_Code", "s0"."DerivedOwned_Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name", 'Owner' AS "Discriminator"
    FROM "Owners" AS "o"
    UNION ALL
    SELECT "s"."Id", "s"."Name", 'SpecialOwner' AS "Discriminator"
    FROM "SpecialOwner" AS "s"
) AS "u"
LEFT JOIN "Owned" AS "o0" ON "u"."Id" = "o0"."OwnerId"
LEFT JOIN "SpecialOwner" AS "s0" ON "u"."Id" = "s0"."Id"
ORDER BY "u"."Id"
""",
            //
            """
SELECT "s0"."DerivedOwned_Address_City"
FROM (
    SELECT "s"."Id"
    FROM "SpecialOwner" AS "s"
) AS "u"
LEFT JOIN "SpecialOwner" AS "s0" ON "u"."Id" = "s0"."Id"
""");
    }

    public override async Task Owned_reference_entity_splitting()
    {
        await base.Owned_reference_entity_splitting();

        AssertSql(
            """
SELECT "o"."Id", "s"."Description", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "OwnerOfSplit" AS "o"
LEFT JOIN "SplitOwnedDescription" AS "s" ON "o"."Id" = "s"."OwnerOfSplitId"
ORDER BY "o"."Id"
""");
    }

    public override async Task Optional_owned_reference_with_only_complex_property()
    {
        await base.Optional_owned_reference_with_only_complex_property();

        AssertSql();
    }

    public override async Task Update_complex_property_of_owned_reference_separate_table()
    {
        await base.Update_complex_property_of_owned_reference_separate_table();

        AssertSql(
            """
@p2='1'
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='Oz' (Nullable = false) (Size = 2)

UPDATE "Owned" SET "Address_City" = @p0, "Address_Country_Name" = @p1
WHERE "OwnerId" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_json_complex_property_of_owned_reference()
    {
        await base.Update_json_complex_property_of_owned_reference();

        AssertSql(
            """
@p0='{"City":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Australia"}}' (Nullable = false) (Size = 84)
@p1='1'

UPDATE "Owners" SET "Address" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Address"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_property_of_json_owned_reference()
    {
        await base.Update_complex_property_of_json_owned_reference();

        AssertSql(
            """
@p0='Melbourne' (Nullable = false) (Size = 9)
@p1='1'

UPDATE "Owners" SET "Owned" = json_set("Owned", '$.Address.City', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_properties_of_json_owned_reference()
    {
        await base.Update_complex_properties_of_json_owned_reference();

        AssertSql(
            """
@p0='{"Name":"Owned1","Address":{"City":"Melbourne","Street":"1 Main St","Country":{"Code":"AU","Name":"Oz"}}}' (Nullable = false) (Size = 105)
@p1='1'

UPDATE "Owners" SET "Owned" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Update_complex_property_of_owned_collection_element()
    {
        await base.Update_complex_property_of_owned_collection_element();

        AssertSql(
            """
@p1='Owned1b' (Nullable = false) (Size = 7)
@p2='1'
@p0='Fremantle' (Nullable = false) (Size = 9)

UPDATE "Owned" SET "Address_City" = @p0
WHERE "Name" = @p1 AND "OwnerId" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o1"."Id", "o1"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM (
    SELECT "o"."Id", "o"."Name"
    FROM "Owners" AS "o"
    WHERE "o"."Id" = 1
    LIMIT 2
) AS "o1"
LEFT JOIN "Owned" AS "o0" ON "o1"."Id" = "o0"."OwnerId"
ORDER BY "o1"."Id", "o0"."OwnerId", "o0"."Name"
""");
    }

    public override async Task Update_complex_property_of_json_owned_collection_element()
    {
        await base.Update_complex_property_of_json_owned_collection_element();

        AssertSql(
            """
@p0='Fremantle' (Nullable = false) (Size = 9)
@p1='1'

UPDATE "Owners" SET "OwnedCollection" = json_set("OwnedCollection", '$[1].Address.City', @p0)
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."OwnedCollection"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Replace_complex_property_of_owned_reference()
    {
        await base.Replace_complex_property_of_owned_reference();

        AssertSql(
            """
@p2='1'
@p0='Brisbane' (Nullable = false) (Size = 8)
@p1='5 Queen St' (Nullable = false) (Size = 10)

UPDATE "Owners" SET "Owned_Address_City" = @p0, "Owned_Address_Street" = @p1
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Replace_owned_reference_with_complex_property()
    {
        await base.Replace_owned_reference_with_complex_property();

        AssertSql(
            """
@p3='1'
@p0='Owned1New' (Nullable = false) (Size = 9)
@p1='Brisbane' (Nullable = false) (Size = 8)
@p2='5 Queen St' (Nullable = false) (Size = 10)

UPDATE "Owners" SET "Owned_Name" = @p0, "Owned_Address_City" = @p1, "Owned_Address_Street" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Delete_owner_with_owned_complex_property_separate_table()
    {
        await base.Delete_owner_with_owned_complex_property_separate_table();

        AssertSql(
            """
@p0='1'

DELETE FROM "Owned"
WHERE "OwnerId" = @p0
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "Owners"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o0"."OwnerId", "o0"."Name", "o0"."Address_City", "o0"."Address_Street", "o0"."Address_Country_Code", "o0"."Address_Country_Name"
FROM "Owners" AS "o"
LEFT JOIN "Owned" AS "o0" ON "o"."Id" = "o0"."OwnerId"
LIMIT 2
""");
    }

    public override async Task Delete_owner_with_owned_complex_property()
    {
        await base.Delete_owner_with_owned_complex_property();

        AssertSql(
            """
@p0='1'

DELETE FROM "Owners"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Name", "o"."Owned_Name", "o"."Owned_Address_City", "o"."Owned_Address_Street", "o"."Owned_Address_Country_Code", "o"."Owned_Address_Country_Name"
FROM "Owners" AS "o"
LIMIT 2
""");
    }
}
