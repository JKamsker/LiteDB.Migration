using LiteDB.Migration.Tests.Migrations._1_0_1;
using LiteDB.Migration.Tests.Migrations._1_0_2;

using LiteDB;
using LiteDB.Migration.Container;

namespace LiteDB.Migration.Tests.Migrations;

internal class DemoMigrationContainer : MigrationContainer
{
    public DemoMigrationContainer(Action<MigrationConfig>? config = null)
        : base(config)
    {
    }

    public override void Configure(MigrationConfig config)
    {
        //config.UseMigrationHistory();

        // Migrationsets contain all the migrations for a specific version
        // The migrations are keyed by the type name of the document
        config
            .WithSet<MigrationSet_1_0_1>()
            .WithSet<MigrationSet_1_0_2>();

        // To create a connection between migrationsets and collections, use the Collection method
        config.Collection<ModelA>("modelA");
        config.Collection<ModelB>("modelB");

        // We could also add the migrations directly to the collection

        //config.Collection("modelA", c => c
        //    .WithMigration<_1_0_1.ModelA_Mapper>()
        //    .WithMigration<_1_0_2.MigrationMapper>()
        //);

        base.Configure(config);
    }
}