namespace LiteDB.Migration.Tests;

using Xunit;
using System;
using System.Text.Json.Nodes;
using LiteDB;

public class MigrationRegistryTests
{
    [Fact]
    public void TestFindMigrationPath_Success()
    {
        // Arrange
        var registry = new MigrationRegistry();
        registry.RegisterMigration(1, 2, bson => new BsonValue()); // Dummy migration function
        registry.RegisterMigration(2, 3, bson => new BsonValue());
        registry.RegisterMigration(3, 4, bson => new BsonValue());
        registry.RegisterMigration(4, 5, bson => new BsonValue());

        // Act
        //var migration = registry.GetMigrationPath("1", "5");
        var migration = registry.GetMigrationPath(1, 5);
        var result = migration.ApplyMigration(new BsonValue()); // Assuming BsonValue is a valid type for demonstration

        // Assert
        // If no exception is thrown and we get a BsonValue result, it means the migration path is successfully found and applied.
        Assert.NotNull(result);
    }

    [Fact]
    public void TestFindMigrationPath_Failure()
    {
        // Arrange
        var registry = new MigrationRegistry();
        //registry.RegisterMigration("1", "2", bson => new BsonValue());
        //registry.RegisterMigration("2", "3", bson => new BsonValue());
        //// Missing migration from "3" to "4"
        //registry.RegisterMigration("4", "5", bson => new BsonValue());

        registry.RegisterMigration(1, 2, bson => new BsonValue());
        registry.RegisterMigration(2, 3, bson => new BsonValue());
        // Missing migration from "3" to "4"
        registry.RegisterMigration(4, 5, bson => new BsonValue());

        // Act & Assert
        //Assert.Throws<InvalidOperationException>(() => registry.GetMigrationPath("1", "5"));
        Assert.Throws<InvalidOperationException>(() => registry.GetMigrationPath(1, 5));
    }

    [Fact]
    public void TestFindMigrationPath_Success_WhenNullMeansDefault()
    {
        // Arrange
        var registry = new MigrationRegistry();

        // Empty means from the default version
        // This happens when the database didn't have migrations before
        //registry.RegisterMigration("", "2", bson => new BsonValue());
        //registry.RegisterMigration("1", "2", bson => new BsonValue());
        //registry.RegisterMigration("2", "3", bson => new BsonValue());
        //registry.RegisterMigration("3", "4", bson => new BsonValue());
        //registry.RegisterMigration("4", "5", bson => new BsonValue());

        registry.RegisterMigration(null, 2, bson => new BsonValue());
        registry.RegisterMigration(1, 2, bson => new BsonValue());
        registry.RegisterMigration(2, 3, bson => new BsonValue());
        registry.RegisterMigration(3, 4, bson => new BsonValue());
        registry.RegisterMigration(4, 5, bson => new BsonValue());

        // Act
        //var migration = registry.GetMigrationPath(null, "5");
        var migration = registry.GetMigrationPath(null, 5);
        var result = migration.ApplyMigration(new BsonValue()); // Assuming BsonValue is a valid type for demonstration

        // Assert
        // If no exception is thrown and we get a BsonValue result, it means the migration path is successfully found and applied.
        Assert.NotNull(result);
    }
}