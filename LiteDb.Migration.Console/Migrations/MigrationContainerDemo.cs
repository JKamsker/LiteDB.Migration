using LiteDB.Migration.ConsoleTest.Migrations._1_0_1;
using LiteDB.Migration.ConsoleTest.Migrations._1_0_2;

using LiteDB;
using LiteDB.Migration.Container;

namespace LiteDB.Migration.ConsoleTest.Migrations;

internal class MigrationContainerDemo : MigrationContainer
{
    public override void Configure(MigrationConfig config)
    {
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
    }
}