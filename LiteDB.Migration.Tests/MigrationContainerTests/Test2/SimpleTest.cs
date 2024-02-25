using LiteDB.Migration.Helpers;

namespace LiteDB.Migration.Tests.MigrationContainerTests.Test2;

public class SimpleTest
{
    [Fact]
    public void Test()
    {
        var opener = DbOpener.Memory();

        // seed db
        using (var db = opener.Open())
        {
            var col = db.GetCollection<ModelX>("ModelX");
            col.Insert(new ModelX { Id = 1, OldProperty = "OldValue" });
        }

        // apply migration
        using (var db = opener.Open())
        {
            ApplyMigration(db);
        }

        // verify migration
        using (var db = opener.Open())
        {
            var col = db.GetCollection<CurrentModel>("ModelX");
            var model = col.FindById(1);
            Assert.NotNull(model);
            Assert.Equal(1, model.Id);
            Assert.Equal("New-OldValue", model.NewProperty);
        }
    }

    [Fact]
    public void Apply_Migrations_Should_Not_Execute_When_Model_Is_Latest()
    {
        var opener = DbOpener.Memory();

        // seed db
        using (var db = opener.Open())
        {
            // Old: Crash, because it believes CurrentModel is still old.
            ApplyMigration(db);

            var col = db.GetCollection<CurrentModel>("ModelX");
            col.Insert(new CurrentModel { Id = 1, NewProperty = "New-OldValue" });
        }

        // apply migration
        using (var db = opener.Open())
        {
            ApplyMigration(db);
        }

        // verify migration
        using (var db = opener.Open())
        {
            var col = db.GetCollection<CurrentModel>("ModelX");
            var model = col.FindById(1);
            Assert.NotNull(model);
            Assert.Equal(1, model.Id);
            Assert.Equal("New-OldValue", model.NewProperty);
        }
    }

  

    private static void ApplyMigration(LiteDatabase db)
    {
        var container = new MigrationContainer(config =>
        {
            config.WithSet<MigrationSet_1_to_2>();

            // Using ModelY as our Marker class
            config.Collection<ModelY>("ModelX");
        });

        container.Apply(db);
    }

    // Initial model
    public class ModelX
    {
        public int Id { get; set; }
        public string OldProperty { get; set; }
    }

    // New model snapshot after some changes
    public class ModelY
    {
        public int Id { get; set; }
        public string NewProperty { get; set; }
    }

    // The latest version of the model, which is actively used in your app
    public class CurrentModel
    {
        public int Id { get; set; }
        public string NewProperty { get; set; }
    }

    public class ModelX_to_ModelY_Mapper : MigrationBase<ModelX, ModelY>
    {
        public override int? From => 1; // Assuming version 1 is where ModelX was used
        public override int To => 2;   // Targeting version 2 for ModelY

        public override ModelY Migrate(ModelX model)
        {
            return new ModelY
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

    public class MigrationSet_1_to_2 : MigrationSet
    {
        public override MigrationSetItem[] Items => new[]
        {
        // ModelY is the marker class, The Mapper will be used to migrate ModelX to ModelY
        //new MigrationSetItem(nameof(ModelY), new ModelX_to_ModelY_Mapper()),

        // Using inline migration
        // MigrationSetItem.Create<ModelY, ModelX, ModelY>(1, 2, x => new ModelY { Id = x.Id, NewProperty = x.OldProperty }),

        // Using inline migration with a custom mapper
        // MigrationSetItem.Create<ModelY>(new ModelX_to_ModelY_Mapper()),

        // Using a custom migration class
        //MigrationSetItem.Create<ModelY, ModelX_to_ModelY_Mapper>(),

        Create<ModelY, ModelX_to_ModelY_Mapper>(),
    };
    }
}