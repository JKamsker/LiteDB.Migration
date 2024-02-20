using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests;

public class MigrationTests
{
    //[Fact]
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_Migrate_Implicit_Success(bool shouldVersionDefault)
    {
        var registry = new MigrationRegistry();

        registry.RegisterMigration(new ModelAtoBMigration());
        registry.RegisterMigration(new ModelAtoBMigration(), null, 1);

        var mapper = new BsonMapper();
        mapper.Entity<ModelA>().Id(x => x.Id);
        mapper.Entity<ModelB>().Id(x => x.Id);

        if (shouldVersionDefault)
        {
            mapper.RegisterMigratable<ModelA>(registry, 0);
        }

        mapper.RegisterMigratable<ModelB>(registry, 1);

        var oldDb = new LiteDatabase(new MemoryStream(), mapper);
        var collection = oldDb.GetCollection<ModelA>("modelA");
        SeedData(collection);

        var collection1 = oldDb.GetCollection<ModelB>("modelA");
        var one = collection1.FindOne(x => x.Name == "John");

        Assert.NotNull(one);
        Assert.Equal("John", one.Name);
        Assert.Equal(30, one.Age);
        Assert.Equal("New York", one.Address.City);
        Assert.Equal("New York", one.Address.Region);
        Assert.Equal("USA", one.Address.Country);
        Assert.Equal("10001", one.Address.PostalCode);
    }

    [Fact]
    public void Test_Migrate_All()
    {
        var registry = new MigrationRegistry();

        registry.RegisterMigration(new ModelAtoBMigration());
        registry.RegisterMigration(new ModelAtoBMigration(), null, 1);

        var mapper = new BsonMapper();
        mapper.Entity<ModelA>().Id(x => x.Id);
        mapper.Entity<ModelB>().Id(x => x.Id);

        mapper.RegisterMigratable<ModelA>(registry, 0);
        mapper.RegisterMigratable<ModelB>(registry, 1);

        var oldDb = new LiteDatabase(new MemoryStream(), mapper);
        var collection = oldDb.GetCollection<ModelA>("modelA");
        SeedData(collection);

        var collection1 = oldDb.GetCollection<ModelB>("modelA");
        var all = collection1.Query().ToArray();

        Assert.NotNull(all);

        var expected = Expectations;

        foreach (var item in all)
        {
            Assert.NotNull(item);
            var expectedItem = expected.FirstOrDefault(x => x.Name == item.Name && x.Age == item.Age);
            Assert.NotNull(expectedItem);
            Assert.Equal(expectedItem.Name, item.Name);
            Assert.Equal(expectedItem.Age, item.Age);
            Assert.Equal(expectedItem.Address.City, item.Address.City);
            Assert.Equal(expectedItem.Address.Region, item.Address.Region);
            Assert.Equal(expectedItem.Address.Country, item.Address.Country);
            Assert.Equal(expectedItem.Address.PostalCode, item.Address.PostalCode);
        }
    }

    [Fact]
    public void Test_Migrate_All1()
    {
        var databaseStream = new MemoryStream();

        BuildOldDatabase(databaseStream);
        MigrateDatabase(databaseStream);
        TestMigrateDatabase(databaseStream);

        void BuildOldDatabase(MemoryStream dbStream)
        {
            using var oldDb = new LiteDatabase(dbStream);
            var collection = oldDb.GetCollection<ModelA>("modelA");
            SeedData(collection);
        }

        void MigrateDatabase(MemoryStream dbStream)
        {
            var registry = new MigrationRegistry();

            registry.RegisterMigration(new ModelAtoBMigration());
            registry.RegisterMigration(new ModelAtoBMigration(), null, 1);

            var mapper = new BsonMapper();
            mapper.Entity<ModelA>().Id(x => x.Id);
            mapper.Entity<ModelB>().Id(x => x.Id);

            mapper.RegisterMigratable<ModelA>(registry, 0);
            mapper.RegisterMigratable<ModelB>(registry, 1);

            using var db = new LiteDatabase(dbStream, mapper);
            // Algo:
            // Load modelA collection as ModelB, which should trigger migration implicitly
            // Save the collection back to the database to another collection temporarily
            // Drop the original collection
            // Rename the temporary collection to the original collection name

            var collection1 = db.GetCollection<ModelB>("modelA");
            var collection2 = db.GetCollection<ModelB>("modelA_temp");
            var collection3 = db.GetCollection("c");

            foreach (var item in collection1.Query().ToEnumerable())
            {
                collection2.Insert(item);
            }

            db.DropCollection("modelA");
            db.RenameCollection("modelA_temp", "modelA");
        }

        // Tests if the migration is successful without creating a MigrationRegistry
        void TestMigrateDatabase(MemoryStream dbStream)
        {
            using var db = new LiteDatabase(dbStream);
            var collection1 = db.GetCollection<ModelB>("modelA");
            var all = collection1.Query().ToArray();

            Assert.NotNull(all);

            var expected = Expectations;

            foreach (var item in all)
            {
                Assert.NotNull(item);
                var expectedItem = expected.FirstOrDefault(x => x.Name == item.Name && x.Age == item.Age);
                Assert.NotNull(expectedItem);
                Assert.Equal(expectedItem.Name, item.Name);
                Assert.Equal(expectedItem.Age, item.Age);
                Assert.Equal(expectedItem.Address.City, item.Address.City);
                Assert.Equal(expectedItem.Address.Region, item.Address.Region);
                Assert.Equal(expectedItem.Address.Country, item.Address.Country);
                Assert.Equal(expectedItem.Address.PostalCode, item.Address.PostalCode);
            }
        }
    }

    private static ModelB[] Expectations = new ModelB[]
    {
        new ModelB
        {
            Name = "John",
            Age = 30,
            Address = new Address
            {
                City = "New York",
                Region = "New York",
                Country = "USA",
                PostalCode = "10001"
            }
        },
        new ModelB
        {
            Name = "Jane",
            Age = 25,
            Address = new Address
            {
                City = "Los Angeles",
                Region = "California",
                Country = "USA",
                PostalCode = "90001"
            }
        },
        new ModelB
        {
            Name = "Doe",
            Age = 35,
            Address = new Address
            {
                City = "San Francisco",
                Region = "California",
                Country = "USA",
                PostalCode = "94016"
            }
        },
        new ModelB
        {
            Name = "Doe",
            Age = 35,
            Address = new Address
            {
                City = "San Francisco",
                Region = "California",
                Country = "USA",
                PostalCode = "94016"
            }
        }
    };

    private static void SeedData(ILiteCollection<ModelA> collection)
    {
        collection.Insert(new ModelA
        {
            Name = "John",
            Age = 30,
            City = "New York",
            Region = "New York",
            Country = "USA",
            PostalCode = "10001"
        });

        collection.Insert(new ModelA
        {
            Name = "Jane",
            Age = 25,
            City = "Los Angeles",
            Region = "California",
            Country = "USA",
            PostalCode = "90001"
        });

        collection.Insert(new ModelA
        {
            Name = "Doe",
            Age = 35,
            City = "San Francisco",
            Region = "California",
            Country = "USA",
            PostalCode = "94016"
        });

        collection.Insert(new ModelA
        {
            Name = "Doe",
            Age = 35,
            City = "San Francisco",
            Region = "California",
            Country = "USA",
            PostalCode = "94016"
        });
    }
}

public class ModelAtoBMigration : MigrationBase<ModelA, ModelB>
{
    public override int? From => 0;
    public override int To => 1;

    public override ModelB Migrate(ModelA model)
    {
        return new ModelB
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            Address = new Address
            {
                City = model.City,
                Region = model.Region,
                Country = model.Country,
                PostalCode = model.PostalCode
            }
        };
    }
}

public class ModelA
{
    [BsonId]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public int Age { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }
    public string PostalCode { get; set; }
}

public class ModelB
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}