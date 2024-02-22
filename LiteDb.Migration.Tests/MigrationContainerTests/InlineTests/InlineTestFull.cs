using LiteDB.Migration.Helpers;
using LiteDB.Migration.Tests.Migrations._1_0_0;
using LiteDB.Migration.Tests.Migrations._1_0_1;
using LiteDB.Migration.Tests.Migrations._1_0_2;

namespace LiteDB.Migration.Tests.MigrationContainerTests.InlineTests;

public class InlineTestFull
{
    [Fact]
    public void Test_InlineMigrations()
    {
        var opener = DbOpener.Memory();

        // seed db
        using (var db = opener.Open())
        {
            Seeder.SeedData(db.GetCollection<ModelA>("modelA"));
        }

        // apply migration
        using (var db = opener.Open())
        {
            var container = new MigrationContainer(config =>
            {
                config.Collection<ModelA>("modelA", x => x
                    .StartWithModel<ModelA_1_0_0>()
                    .WithMigration(model => new
                    {
                        model.Id,
                        model.Name,
                        model.Age,
                        Address = new
                        {
                            model.City,
                            model.Region,
                            model.Country,
                            model.PostalCode
                        }
                    })
                    .WithMigration(document => new
                    {
                        document.Id,
                        document.Address,
                        Person = new
                        {
                            document.Name,
                            document.Age
                        }
                    })
                );
            });

            container.Apply(db);
        }

        // verify migration
        using (var db = opener.Open())
        {
            var collection = db.GetCollection<ModelA_V1>("modelA");
            var items = collection.FindAll().ToList();
        }
    }
}