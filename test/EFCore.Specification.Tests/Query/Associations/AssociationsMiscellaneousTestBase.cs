// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsMiscellaneousTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    #region Simple filters

    [Fact]
    public virtual Task Where_on_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Int == 8));

    [Fact]
    public virtual Task Where_on_optional_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate!.Int == 8));

    [Fact]
    public virtual Task Where_on_nested_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Int == 8));

    #endregion Simple filters

    #region GroupBy element

    [Fact]
    public virtual Task Where_on_root_property_on_GroupBy_First()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
            .Where(e => e.Id > 1));

    [Fact]
    public virtual Task Where_on_associate_scalar_property_on_GroupBy_First()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
            .Where(e => e.RequiredAssociate.Int == 8));

    [Fact]
    public virtual Task OrderBy_associate_scalar_property_on_GroupBy_First()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => g.OrderByDescending(e => e.Id).First())
                .OrderBy(e => e.RequiredAssociate.Int)
                .ThenBy(e => e.Id),
            assertOrder: true);

    #endregion GroupBy element
}
