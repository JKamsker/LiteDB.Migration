using LiteDB.Migration.Tests.Migrations;

using LiteDB;

using System.Diagnostics;
using LiteDB.Migration.Container;

namespace LiteDB.Migration.Tests.MigrationContainerTests;

public class MigrationContainerTest
{
    [Fact]
    public void Test_Migrate_Success()
    {
        Func<LiteDatabase> dbOpener = CreateDbOpenerMS();
        //Func<LiteDatabase> dbOpener = CreateDbOpenerFile();

        CreateDbStream(dbOpener);
        UpdateDbStream(dbOpener);

        using var newDb = dbOpener();

        var collection = newDb.GetCollection<ModelA_V1>("modelA");
        var collection1 = newDb.GetCollection<ModelB_V1>("modelB");
        var items = collection.FindAll().ToList();
        var items_modelb = collection1.FindAll().ToList();

        var expectations = Seeder.GetModelB_Expectations().ToList();
        Assert.Equal(expectations.Count, items_modelb.Count);

        foreach (var item in items_modelb)
        {
            var expectation = expectations.Any(x => x.ContentEquals(item));
            Assert.True(expectation, $"Expectation failed for {item}. Mapped model doesn't match any expectation.");
        }
    }

    [Fact]
    public void Test_Migrate_Use_MigrationHistory()
    {
        Func<LiteDatabase> dbOpener = CreateDbOpenerMS();
        //Func<LiteDatabase> dbOpener = CreateDbOpenerFile();

        CreateDbStream(dbOpener);
        UpdateDbStream(dbOpener, x => x.UseMigrationHistory());

        using var newDb = dbOpener();

        var collection = newDb.GetCollection<ModelA_V1>("modelA");
        var collection1 = newDb.GetCollection<ModelB_V1>("modelB");
        var items = collection.FindAll().ToList();
        var items_modelb = collection1.FindAll().ToList();

        var expectations = Seeder.GetModelB_Expectations().ToList();
        Assert.Equal(expectations.Count, items_modelb.Count);

        foreach (var item in items_modelb)
        {
            var expectation = expectations.Any(x => x.ContentEquals(item));
            Assert.True(expectation, $"Expectation failed for {item}. Mapped model doesn't match any expectation.");
        }
    }

    // In this test:
    // We got 3 versions: 1,2,3 where 1 is the oldest and assumed to be the default one and 3 is the latest.
    // We will first create a db with version 1, then apply migration to version 2.
    // Then we will add an item to the db with version 2.
    // Then we will apply migration to version 3.

    // I think:
    // 1->2 should insert the __version field into the migrated documents
    // The inserted item won't have that field
    // therefore, 2->3 will fail and we need to fix that
    // This test is to support my hypothesis
    [Fact]
    public void Two_Migrations_Should_Succeed()
    {
        Func<LiteDatabase> dbOpener = CreateDbOpenerMS();
        //Func<LiteDatabase> dbOpener = CreateDbOpenerFile();

        CreateDbStream(dbOpener);

        using (var dbv1 = dbOpener())
        {
            new MigrationContainer(config =>
            {
                config
                    .WithSet<Migrations._1_0_1.MigrationSet_1_0_1>();

                // To create a connection between migrationsets and collections, use the Collection method
                config.Collection<ModelA>("modelA");
            }).Apply(dbv1);
        }

        // Now we will add an item (ModelA v1) to the db
        using (var dbv2 = dbOpener())
        {
            var collection = dbv2.GetCollection<Migrations._1_0_1.ModelA_1_0_1>("modelA");
            collection.Insert(Migrations._1_0_1.ModelA_1_0_1.CreateExample());
        }

        // Now we will migrate it again, but this time to version 2
        using (var dbv2 = dbOpener())
        {
            new MigrationContainer(config =>
            {
                config
                    .WithSet<Migrations._1_0_1.MigrationSet_1_0_1>()
                    .WithSet<Migrations._1_0_2.MigrationSet_1_0_2>();

                // To create a connection between migrationsets and collections, use the Collection method
                config.Collection<ModelA>("modelA");
            }).Apply(dbv2);
        }

        // Assert
        using (var dbv3 = dbOpener())
        {
            var source = Migrations._1_0_1.ModelA_1_0_1.CreateExample();
            var expected = new Migrations._1_0_2.ModelA_Mapper().Migrate(source);

            var collection = dbv3.GetCollection<ModelA_V1>("modelA");
            var items = collection.FindAll().ToList();

            var newItem = items.FirstOrDefault(x => x.Person.Name == expected.Person.Name);

            Assert.NotNull(items);
            //Assert.True(expected.ContentEquals(newItem), $"Expectation failed for {items}. Mapped model doesn't match any expectation.");
            /*
             return
               item.Person.Name == Person.Name &&
               item.Person.Age == Person.Age &&
               item.Address.City == Address.City &&
               item.Address.Region == Address.Region &&
               item.Address.Country == Address.Country &&
               item.Address.PostalCode == Address.PostalCode;
             */

            Assert.Equal(expected.Person.Name, newItem.Person.Name);
            Assert.Equal(expected.Person.Age, newItem.Person.Age);
            Assert.Equal(expected.Address.City, newItem.Address.City);
            Assert.Equal(expected.Address.Region, newItem.Address.Region);
            Assert.Equal(expected.Address.Country, newItem.Address.Country);
            Assert.Equal(expected.Address.PostalCode, newItem.Address.PostalCode);

        }
    }

    private static Func<LiteDatabase> CreateDbOpenerFile()
    {
        if (File.Exists("lite.db"))
        {
            File.Delete("lite.db");
        }

        if (File.Exists("lite-log.db"))
        {
            File.Delete("lite-log.db");
        }

        Func<LiteDatabase> dbOpener = () => new LiteDatabase("lite.db");
        return dbOpener;
    }

    private static Func<LiteDatabase> CreateDbOpenerMS()
    {
        var dbstream = new MemoryStream();
        Func<LiteDatabase> dbOpener = () => new LiteDatabase(dbstream);
        return dbOpener;
    }

    private static void CreateDbStream(Func<LiteDatabase> dbOpener)
    {
        using var oldDb = dbOpener();
        Seeder.SeedData(oldDb.GetCollection<ModelA>("modelA"));
        Seeder.SeedData(oldDb.GetCollection<ModelB>("modelB"));
    }

    private static void UpdateDbStream(Func<LiteDatabase> dbOpener, Action<MigrationConfig>? config = null)
    {
        //using var oldDb = new LiteDatabase(dbstream);
        using var oldDb = dbOpener();
        var sw = Stopwatch.StartNew();
        var container = new DemoMigrationContainer(config);
        container.Apply(oldDb);
        Console.WriteLine($"Migration took {sw.ElapsedMilliseconds}ms");
    }
}