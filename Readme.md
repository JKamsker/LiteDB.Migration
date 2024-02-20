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

Get started by following the steps below. 
For a full example, see the [Tests](https://github.com/JKamsker/LiteDb.Migration/blob/master/LiteDb.Migration.Tests/MigrationContainerTests/Test1/MigrationContainerTest.cs#L14)

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

// Current model (Should always be ModelV[n])
public class Model
{
    public int Id { get; set; }
    public string NewProperty { get; set; }
}
```

2. **Seed Data**: Seed your database with the original model.

```csharp
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var collection = db.GetCollection<ModelV1>("YourCollectionName");
    collection.Insert(new ModelV1 { Id = 1, OldProperty = "Old" });
}
```

3. **Define and apply Migrations**: Define your migrations using the `MigrationContainer` class and apply them using the `MigrationContainer` class.

```csharp
 // apply migration
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var container = new MigrationContainer(config =>
    {
        config.Collection<Model>("modelA", x => x
            .WithMigrationStart<ModelV1>()
            .WithInlineMigration(model => new ModelV2
            {
                Id = model.Id,
                NewProperty = $"New-{model.OldProperty}"
            })
            // You can add more migrations here
        );
    });

    container.Apply(db);
}
```

4. **Query Data**: Query your database using the new model.

```csharp
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var collection = db.GetCollection<Model>("modelA");
    var result = collection.FindAll().First();
    Console.WriteLine(result.NewProperty); // Output: New-Old
}
```
## Advanced Usage
For larger (enterprise) applications, you may want to use a more structured approach to migrations. In this case, you can use the please see the [Full Example](https://github.com/JKamsker/LiteDb.Migration/blob/master/LiteDb.Migration.Tests/MigrationContainerTests/Test1/MigrationContainerTest.cs#L14) (Wiki page coming soon).

## Contributing

Contributions are welcome! Feel free to submit pull requests, report issues, or suggest new features.

## License

LiteDB.Migration is released under the MIT License. See the LICENSE file for more details.