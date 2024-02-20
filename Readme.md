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
// Initial model
public class EntryModel
{
    public int Id { get; set; }
    public string OldProperty { get; set; }
}

// The latest version of the model, which is actively used in your app
public class CurrentModel
{
    public int Id { get; set; }
    public string NewestProperty { get; set; }
}
```

2. **Seed Data**: Seed your database with the original model.

```csharp
 // seed db
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var col = db.GetCollection<EntryModel>("ModelX");
    col.Insert(new EntryModel { Id = 1, OldProperty = "OldValue" });
}
```

3. **Define and apply Migrations**: Define your migrations using the `MigrationContainer` class and apply them using the `MigrationContainer` class.<br/>
Whenever you want to change the model, you can define a new migration using the `WithInlineMigration` method. 

```csharp
 // apply migration
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var container = new MigrationContainer(migConfig =>
    {
        migConfig.Collection<CurrentModel>("ModelX", config => config
            .WithMigrationStart<EntryModel>()
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
```

1. **Query Data**: Query your database using the new model.

```csharp
// verify migration
using (var db = new LiteDatabase("YourDatabase.db"))
{
    var col = db.GetCollection<CurrentModel>("ModelX");
    var model = col.FindById(1);

    Console.WriteLine($"Id: {model.Id}, NewestProperty: {model.NewestProperty}");
    if (model.Id != 1 || model.NewestProperty != "New-New-OldValue")
    {
        throw new Exception("Migration failed");
    }
    else
    {
        Console.WriteLine("Migration succeeded");
    }
}
```
## Advanced Usage
For larger (enterprise) applications, you may want to use a more structured approach to migrations. In this case, you can use the please see the [Full Example](https://github.com/JKamsker/LiteDb.Migration/blob/master/LiteDb.Migration.Tests/MigrationContainerTests/Test1/MigrationContainerTest.cs#L14) (Wiki page coming soon).

## Contributing

Contributions are welcome! Feel free to submit pull requests, report issues, or suggest new features.

## License

LiteDB.Migration is released under the MIT License. See the LICENSE file for more details.