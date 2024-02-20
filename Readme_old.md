# LiteDB.Migration

LiteDB.Migration is a lightweight, easy-to-use library designed to facilitate schema migrations for LiteDB databases in .NET applications. It provides a structured way to evolve your database schema over time, ensuring that your application can safely and efficiently migrate data as your models change.

## Features

- **Flexible Migration Paths**: Migrate data across multiple versions with ease.
- **Simple API**: Define your migrations using a simple API.
- **Implicit and Explicit Migrations**: Migrate all at once or in a Lazy way.

## Getting Started

### Installation

LiteDB.Migration is available as a NuGet package. You can install it via the NuGet Package Manager or the CLI:

```bash
dotnet add package LiteDB.Migration
```

### Basic Usage

Get started by following the steps below. For a full example, see the [Tests](https://github.com/JKamsker/LiteDb.Migration/blob/master/LiteDb.Migration.Tests/MigrationContainerTests/Test1/MigrationContainerTest.cs#L14)

1. **Define Your Models**: Start by defining your document models. For each version change that requires a migration, prepare a new version of the model.

```csharp
// Original model
public class ModelV1
{
    public int Id { get; set; }
    public string OldProperty { get; set; }
}

// Updated model
public class ModelV2
{
    public int Id { get; set; }
    public string NewProperty { get; set; }
}
```

2. **Implement Migrations**: Create a migration class for each model transformation. Inherit from `MigrationBase<TFrom, TTo>` and implement the migration logic.

```csharp
public class ModelV1_to_ModelV2_Mapper : MigrationBase<ModelV1, ModelV2>
{
    public override int? From => 1; // Original version
    public override int To => 2;   // New version

    public override ModelV2 Migrate(ModelV1 model)
    {
        return new ModelV2
        {
            Id = model.Id,
            NewProperty = $"New-{model.OldProperty}"
        };
    }
}
```

3. **Create a Migration Set**: Create a migration set to group your migrations together. Implement the `IMigrationSet` interface and return a list of your migrations.

```csharp
public class MigrationSet_1_to_2 : IMigrationSet
{
    public MigrationSetItem[] Items => new[]
    {
        // ModelV2 is the marker class, The Mapper will be used to migrate Model to ModelV2
        new MigrationSetItem(nameof(ModelV2), new ModelX_to_ModelY_Mapper())
    };
}
```

4. **Configure and Apply Migrations**: Use the `MigrationContainer` to configure and apply your migrations.

```csharp
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var container = new MigrationContainer(config =>
    {
        config.WithSet<YourMigrationSet>();
        
        // The generic type is the type of the marker class
        config.Collection<ModelV2>("YourCollectionName");
    });

    container.Apply(db);
}
```

## Contributing

Contributions are welcome! Feel free to submit pull requests, report issues, or suggest new features.

## License

LiteDB.Migration is released under the MIT License. See the LICENSE file for more details.