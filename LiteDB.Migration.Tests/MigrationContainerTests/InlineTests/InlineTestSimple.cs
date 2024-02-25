using LiteDB.Migration.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests.MigrationContainerTests.InlineTests;

public class InlineTestSimple
{
    [Fact]
    public void Test_InlineMigrations()
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
            var container = new MigrationContainer(config =>
            {
                //config.Collection<ModelY>("ModelX", x => x.WithMigration<ModelX_to_ModelY_Mapper>());

                config.Collection<ModelV2>("ModelX", x => x
                    .WithMigration<ModelV1, ModelV2>(1, 2, x => new ModelV2
                    {
                        Id = x.Id,
                        NewProperty = "New-" + x.OldProperty
                    }, x => x.WithBsonMapper(BsonMapper.Global))
                    .WithMigration<ModelV2, ModelV3>(2, 3, x => new ModelV3
                    {
                        Id = x.Id,
                        NewestProperty = "New-" + x.NewProperty
                    }, x => x.WithBsonMapper(BsonMapper.Global))

                );
            });

            container.Apply(db);
        }

        // verify migration
        using (var db = opener.Open())
        {
            var col = db.GetCollection<CurrentModel>("ModelX");
            var model = col.FindById(1);
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

    // New model snapshot after some changes
    public class ModelV2
    {
        public int Id { get; set; }
        public string NewProperty { get; set; }
    }

    public class ModelV3
    {
        public int Id { get; set; }
        public string NewestProperty { get; set; }
    }

    // The latest version of the model, which is actively used in your app
    public class CurrentModel
    {
        public int Id { get; set; }
        public string NewestProperty { get; set; }
    }

    public class ModelX_to_ModelY_Mapper : MigrationBase<ModelV1, ModelV2>
    {
        public override int? From => 1; // Assuming version 1 is where ModelX was used
        public override int To => 2;   // Targeting version 2 for ModelY

        public override ModelV2 Migrate(ModelV1 model)
        {
            return new ModelV2
            {
                Id = model.Id,
                NewProperty = TransformProperty(model.OldProperty)
            };
        }

        private string TransformProperty(string oldProperty)
        {
            // Implement your transformation logic here
            // For example, prepend "New-" to the old property value
            return $"New-{oldProperty}";
        }
    }
}