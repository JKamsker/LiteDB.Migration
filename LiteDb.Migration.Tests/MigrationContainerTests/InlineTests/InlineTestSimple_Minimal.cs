using LiteDB.Migration.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests.MigrationContainerTests.InlineTests;

public class InlineTestSimple_Minimal
{
    [Fact]
    public void Test_InlineMigrations_Simple_Minimal()
    {
        var opener = DbOpener.Memory();

        // seed db
        using (var db = opener.Open())
        {
            var col = db.GetCollection<ModelV1>("ModelX");
            col.Insert(new ModelV1 { Id = 1, OldProperty = "OldValue" });
        }

        // apply migration
        using (var db = opener.Open())
        {
            var container = new MigrationContainer(migConfig =>
            {
                //config.Collection<ModelY>("ModelX", x => x.WithMigration<ModelX_to_ModelY_Mapper>());

                migConfig.Collection<CurrentModel>("ModelX", config => config
                    .WithMigrationStart<ModelV1>()
                    .WithInlineMigration(x => new
                    {
                        x.Id,
                        NewProperty = "New-" + x.OldProperty
                    })
                    .WithInlineMigration(x => new
                    {
                        x.Id,
                        NewestProperty = "New-" + x.NewProperty
                    })
                );
            });

            container.Apply(db);
        }

        // verify migration
        using (var db = opener.Open())
        {
            var col = db.GetCollection<CurrentModel>("ModelX");
            var model = col.FindById(1);
            var all = col.FindAll().ToList();

            Assert.NotNull(model);
            Assert.Equal(1, model.Id);
            Assert.Equal("New-New-OldValue", model.NewestProperty);
        }
    }

    // Initial model
    public class ModelV1
    {
        public int Id { get; set; }
        public string OldProperty { get; set; }
    }

    public class CurrentModel
    {
        public int Id { get; set; }
        public string NewestProperty { get; set; }
    }
}