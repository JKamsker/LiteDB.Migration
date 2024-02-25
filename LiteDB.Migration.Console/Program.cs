using LiteDB.Migration.ConsoleTest.Migrations;

using LiteDB;

using System.Diagnostics;
using LiteDB.Migration.ConsoleTest.Migrations._1_0_1;
using LiteDB.Migration.ConsoleTest.Migrations._1_0_2;

namespace LiteDB.Migration.ConsoleTest;

internal class Program
{
    private static async Task Main(string[] args)
    {
        /*
            Migration took 804ms
            Migration took 662ms
            Migration took 626ms
            Migration took 688ms

            With batching:
            Migration took 403ms
            Migration took 405ms
            Migration took 315ms
            Migration took 307ms
            Migration took 295ms
            Migration took 322ms
            Migration took 293ms
            Migration took 330ms
         */

        while (true)
        {
            var dbstream = new MemoryStream();
            Func<LiteDatabase> dbOpener = () => new LiteDatabase(dbstream);

            //if (File.Exists("lite.db"))
            //{
            //    File.Delete("lite.db");
            //}

            //if (File.Exists("lite-log.db"))
            //{
            //    File.Delete("lite-log.db");
            //}

            //Func<LiteDatabase> dbOpener = () => new LiteDatabase("lite.db");

            CreateDbStream(dbOpener);
            UpdateDbStream(dbOpener);

            using var newDb = dbOpener();

            var collection = newDb.GetCollection<ModelA_V1>("modelA");
            var collection1 = newDb.GetCollection<ModelB_V1>("modelB");
            var items = collection.FindAll().ToList();
            var items1 = collection1.FindAll().ToList();

            await Task.Delay(1000);
        }
    }

    private static void CreateDbStream(Func<LiteDatabase> dbOpener)
    {
        using var oldDb = dbOpener();
        Seeder.SeedData(oldDb.GetCollection<ModelA>("modelA"), 1000);
        Seeder.SeedData(oldDb.GetCollection<ModelB>("modelB"), 1000);
    }

    private static void UpdateDbStream(Func<LiteDatabase> dbOpener)
    {
        //using var oldDb = new LiteDatabase(dbstream);
        using var oldDb = dbOpener();
        var sw = Stopwatch.StartNew();
        var container = new MigrationContainer(config =>
        {
            config
                .WithSet<MigrationSet_1_0_1>()
                .WithSet<MigrationSet_1_0_2>();

            // To create a connection between migrationsets and collections, use the Collection method
            config.Collection<ModelA>("modelA");
            config.Collection<ModelB>("modelB");
        });

        container.Apply(oldDb);
        Console.WriteLine($"Migration took {sw.ElapsedMilliseconds}ms");
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

public class ModelA_V1
{
    public Guid Id { get; set; }
    public Person Person { get; set; }
    public Address Address { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}

internal class ModelB
{
    [BsonId]
    public Guid Id { get; set; }

    public string ProductName { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public double Price { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }
}

internal class ModelB_V1
{
    [BsonId]
    public Guid Id { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public OrderAddress Address { get; set; }

    public ProductInfo ProductInfo { get; set; }
}

internal class ProductInfo
{
    public string ProductName { get; set; }
    public double Price { get; set; }
}

internal class OrderAddress
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }
}