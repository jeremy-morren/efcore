// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedComplexPropertiesQueryRelationalTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "OwnedComplexPropertiesQueryTests";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    [Fact]
    public virtual async Task Owned_reference_table_splitting()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_separate_table()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_with_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_json()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_collection()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ob.HasKey("OwnerId", nameof(Owned.Name));
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwnedCollections(owners);
    }

    [Fact]
    public virtual async Task Owned_collection_json()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            onConfiguring: b => b.ConfigureWarnings(w => w.Log(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning)),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwnedCollections(owners);
    }

    [Fact]
    public virtual async Task Filter_on_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owner = await context.Owners.SingleAsync(e => e.Owned.Address.Country.Code == "DE");

        Assert.Equal(2, owner.Id);
    }

    [Fact]
    public virtual async Task Project_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var addresses = await context.Owners.OrderBy(e => e.Id).Select(e => e.Owned.Address).ToListAsync();

        Assert.Collection(
            addresses,
            a => Assert.Equal("Sydney", a.City),
            a => Assert.Equal("Berlin", a.City));
    }

    [Fact]
    public virtual async Task Project_nested_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var countries = await context.Owners.OrderBy(e => e.Id).Select(e => e.Owned.Address.Country).ToListAsync();

        Assert.Collection(
            countries,
            c => Assert.Equal("AU", c.Code),
            c => Assert.Equal("DE", c.Code));
    }

    [Fact]
    public virtual async Task Owned_reference_in_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owneds = await context.Owners.AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 1)
            .Select(e => e.Owned)
            .ToListAsync();

        var owned = Assert.Single(owneds);
        Assert.Equal("Berlin", owned.Address.City);
        Assert.Equal("DE", owned.Address.Country.Code);
    }

    [Fact]
    public virtual async Task Owned_reference_with_json_complex_property_in_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owneds = await context.Owners.AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 1)
            .Select(e => e.Owned)
            .ToListAsync();

        var owned = Assert.Single(owneds);
        Assert.Equal("Berlin", owned.Address.City);
        Assert.Equal("DE", owned.Address.Country.Code);
    }

    [Fact]
    public virtual async Task Owned_reference_with_distinct()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.Distinct().OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_in_union()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.Where(e => e.Id == 1)
            .Union(context.Owners.Where(e => e.Id == 2))
            .OrderBy(e => e.Id)
            .ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual Task Owned_reference_on_TPH_owner()
        => AssertOwnedReferencesWithDerivedOwner(b => { });

    [Fact]
    public virtual Task Owned_reference_on_TPT_owner()
        => AssertOwnedReferencesWithDerivedOwner(b => b.UseTptMappingStrategy());

    [Fact]
    public virtual Task Owned_reference_on_TPC_owner()
        => AssertOwnedReferencesWithDerivedOwner(b =>
        {
            b.UseTpcMappingStrategy();
            b.OwnsOne(e => e.Owned, ob => ob.ToTable("Owned"));
        });

    [Fact]
    public virtual async Task Owned_reference_entity_splitting()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfSplit>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.SplitToTable("SplitOwnedDescription", t => t.Property(o => o.Description));
                        ob.ComplexProperty(o => o.Address);
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfSplit
                    {
                        Id = 1,
                        Owned = new SplitOwned
                        {
                            Name = "Owned1",
                            Description = "Description1",
                            Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new OwnerOfSplit
                    {
                        Id = 2,
                        Owned = new SplitOwned
                        {
                            Name = "Owned2",
                            Description = "Description2",
                            Address = CreateOwned("", "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Set<OwnerOfSplit>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            owners,
            o =>
            {
                Assert.Equal("Description1", o.Owned.Description);
                Assert.Equal("Sydney", o.Owned.Address.City);
                Assert.Equal("AU", o.Owned.Address.Country.Code);
            },
            o =>
            {
                Assert.Equal("Description2", o.Owned.Description);
                Assert.Equal("Berlin", o.Owned.Address.City);
                Assert.Equal("DE", o.Owned.Address.Country.Code);
            });
    }

    [Fact(Skip = "Issue #35701")]
    public virtual async Task Optional_owned_reference_with_only_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfOptional>(b => b.OwnsOne(e => e.Holder, ob => ob.ComplexProperty(o => o.Address)));
            },
            onConfiguring: b => b.ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning)),
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new AddressHolder { Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address }
                    },
                    new OwnerOfOptional { Id = 2 });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Set<OwnerOfOptional>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            owners,
            o => Assert.Equal("Sydney", o.Holder!.Address.City),
            o => Assert.Null(o.Holder));
    }

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(ConfigureOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference_separate_table()
        => AssertUpdateOwnedReference(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            nested: true);

    [Fact]
    public virtual Task Update_json_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            nested: false);

    [Fact]
    public virtual Task Update_complex_property_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: false);

    [Fact]
    public virtual Task Update_complex_properties_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_of_owned_collection_element()
        => AssertUpdateOwnedCollectionElement(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ob.HasKey("OwnerId", nameof(Owned.Name));
                        ob.ComplexProperty(o => o.Address);
                    });
            }));

    [Fact]
    public virtual Task Update_complex_property_of_json_owned_collection_element()
        => AssertUpdateOwnedCollectionElement(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            onConfiguring: b => b.ConfigureWarnings(w => w.Log(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning)));

    [Fact]
    public virtual async Task Replace_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address = CreateOwned("", "5 Queen St", "Brisbane", "Australia", "AU").Address;

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("5 Queen St", owner.Owned.Address.Street);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task Replace_owned_reference_with_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned = CreateOwned("Owned1New", "5 Queen St", "Brisbane", "Australia", "AU");

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Owned1New", owner.Owned.Name);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task Delete_owner_with_owned_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Remove(await context.Owners.SingleAsync(e => e.Id == 1));

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(2, (await context.Owners.SingleAsync()).Id);
        }
    }

    private async Task AssertUpdateOwnedReference(Action<ModelBuilder> onModelCreating, bool nested)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(onModelCreating: onModelCreating, seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address.City = "Melbourne";
            if (nested)
            {
                owner.Owned.Address.Country.Name = "Oz";
            }

            context.ChangeTracker.DetectChanges();
            var addressEntry = context.Entry(owner.Owned).ComplexProperty(o => o.Address);
            Assert.True(addressEntry.IsModified);
            Assert.Equal("Sydney", addressEntry.Property(a => a.City).OriginalValue);
            Assert.Equal(nested, addressEntry.ComplexProperty(a => a.Country).Property(c => c.Name).IsModified);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Melbourne", owner.Owned.Address.City);
            Assert.Equal(nested ? "Oz" : "Australia", owner.Owned.Address.Country.Name);
            Assert.Equal("1 Main St", owner.Owned.Address.Street);
        }
    }

    private async Task AssertUpdateOwnedCollectionElement(
        Action<ModelBuilder> onModelCreating,
        Action<DbContextOptionsBuilder>? onConfiguring = null)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: onModelCreating, onConfiguring: onConfiguring, seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.OwnedCollection.Single(e => e.Name == "Owned1b").Address.City = "Fremantle";

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal(
                ["Sydney", "Fremantle"],
                owner.OwnedCollection.OrderBy(e => e.Name).Select(e => e.Address.City));
        }
    }

    private static void ConfigureJsonOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(
                e => e.Owned, ob =>
                {
                    ob.ToJson();
                    ob.ComplexProperty(o => o.Address);
                });
        });

    private static void ConfigureOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
        });

    private async Task AssertOwnedReferencesWithDerivedOwner(Action<EntityTypeBuilder<Owner>> configureMappingStrategy)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Entity<Owner>(b =>
                {
                    configureMappingStrategy(b);
                    b.Ignore(e => e.OwnedCollection);
                    b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
                });
                mb.Entity<SpecialOwner>(b => b.OwnsOne(e => e.DerivedOwned, ob => ob.ComplexProperty(o => o.Address)));
            },
            onConfiguring: b => b.ConfigureWarnings(w => w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning)),
            seed: context =>
            {
                context.Owners.AddRange(
                    new Owner
                    {
                        Id = 1,
                        Name = "Owner1",
                        Owned = CreateOwned("Owned1", "1 Main St", "Sydney", "Australia", "AU")
                    },
                    new SpecialOwner
                    {
                        Id = 2,
                        Name = "Owner2",
                        Owned = CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE"),
                        DerivedOwned = CreateOwned("Derived2", "4 Elizabeth St", "Hobart", "Australia", "AU")
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
        Assert.Equal("Hobart", Assert.IsType<SpecialOwner>(owners[1]).DerivedOwned.Address.City);

        var cities = await context.Owners.OfType<SpecialOwner>().Select(e => e.DerivedOwned.Address.City).ToListAsync();

        Assert.Equal(["Hobart"], cities);
    }

    private static void AssertOwners(List<Owner> owners)
        => Assert.Collection(
            owners,
            o =>
            {
                Assert.Equal("Owned1", o.Owned.Name);
                Assert.Equal("1 Main St", o.Owned.Address.Street);
                Assert.Equal("Sydney", o.Owned.Address.City);
                Assert.Equal("Australia", o.Owned.Address.Country.Name);
                Assert.Equal("AU", o.Owned.Address.Country.Code);
            },
            o =>
            {
                Assert.Equal("Owned2", o.Owned.Name);
                Assert.Equal("2 Hauptstrasse", o.Owned.Address.Street);
                Assert.Equal("Berlin", o.Owned.Address.City);
                Assert.Equal("Germany", o.Owned.Address.Country.Name);
                Assert.Equal("DE", o.Owned.Address.Country.Code);
            });

    private static void AssertOwnedCollections(List<Owner> owners)
        => Assert.Collection(
            owners,
            o => Assert.Equal(
                ["Sydney", "Perth"],
                o.OwnedCollection.OrderBy(e => e.Name).Select(e => e.Address.City)),
            o => Assert.Equal(
                ["Berlin"],
                o.OwnedCollection.Select(e => e.Address.City)));

    private static Task Seed(Context context)
    {
        context.Owners.AddRange(
            new Owner
            {
                Id = 1,
                Name = "Owner1",
                Owned = CreateOwned("Owned1", "1 Main St", "Sydney", "Australia", "AU"),
                OwnedCollection =
                [
                    CreateOwned("Owned1a", "1 Main St", "Sydney", "Australia", "AU"),
                    CreateOwned("Owned1b", "3 Hay St", "Perth", "Australia", "AU")
                ]
            },
            new Owner
            {
                Id = 2,
                Name = "Owner2",
                Owned = CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE"),
                OwnedCollection = [CreateOwned("Owned2a", "2 Hauptstrasse", "Berlin", "Germany", "DE")]
            });

        return context.SaveChangesAsync();
    }

    private static Owned CreateOwned(string name, string street, string city, string country, string code)
        => new()
        {
            Name = name,
            Address = new Address
            {
                Street = street,
                City = city,
                Country = new Country { Name = country, Code = code }
            }
        };

    protected class Context(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners { get; set; } = null!;
    }

    public class Owner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Owned Owned { get; set; } = null!;
        public List<Owned> OwnedCollection { get; set; } = [];
    }

    public class SpecialOwner : Owner
    {
        public Owned DerivedOwned { get; set; } = null!;
    }

    public class OwnerOfOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public AddressHolder? Holder { get; set; }
    }

    public class OwnerOfSplit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public SplitOwned Owned { get; set; } = null!;
    }

    public class SplitOwned
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }

    public class AddressHolder
    {
        public Address Address { get; set; } = null!;
    }

    public class Owned
    {
        public string Name { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }

    public class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public Country Country { get; set; } = null!;
    }

    public class Country
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
