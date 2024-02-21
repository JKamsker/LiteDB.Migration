namespace LiteDB.Migration.Container;

internal class CollectionConfigData
{
    public List<MigrationBase> Migrations { get; private protected set; }

    public int? ImplicitLatestVersion { get; set; }

    public int? ExplicitLatestVersion { get; set; }

    public int? LatestVersion
    {
        get => ExplicitLatestVersion ?? ImplicitLatestVersion;
    }

    public CollectionConfigData()
    {
        Migrations = new();
    }
}

public class CollectionConfig<TCurrent> : CollectionConfig
    where TCurrent : class
{
    public CollectionConfig(CollectionConfig config)
        : base(config)
    {
    }

    /// <summary>
    /// Starts a new migration for the source model.
    /// </summary>
    public CollectionConfig WithMigrationStart<TSourceModel>(Func<InlineCollectionConfigStarter<TSourceModel>, ITargeModel<TCurrent>> conf)
        where TSourceModel : class
    {
        var starter = new InlineCollectionConfigStarter<TSourceModel>(this);
        var target = conf(starter);

        return this;
    }
}

public class CollectionConfig
{
    public List<MigrationBase> Migrations => _data.Migrations;
    public int? LatestVersion => _data.LatestVersion;

    //public int? LatestVersion
    //{
    //    get => _latestVersion ?? _implicitLatestVersion;
    //    private set => _latestVersion = value;
    //}

    //protected int? _implicitLatestVersion;
    //protected int? _latestVersion;

    private CollectionConfigData _data;

    public CollectionConfig()
    {
        _data = new();
    }

    public CollectionConfig(CollectionConfig config)
    {
        _data = config._data;
    }

    public CollectionConfig WithLatestVersion(int version)
    {
        _data.ExplicitLatestVersion = version;
        return this;
    }

    public CollectionConfig WithMigrations(IEnumerable<MigrationBase> migrations, bool deduplicate = false)
    {
        foreach (var migration in migrations)
        {
            if (deduplicate && Migrations.Any(x => x.From == migration.From && x.To == migration.To))
            {
                continue;
            }

            Migrations.Add(migration);
            //_implicitLatestVersion = migration.To;
            _data.ImplicitLatestVersion = migration.To;
        }

        return this;
    }

    public CollectionConfig WithMigration<T>()
        where T : MigrationBase, new()
    {
        var migration = new T();
        Migrations.Add(migration);

        //_implicitLatestVersion = migration.To;
        _data.ImplicitLatestVersion = migration.To;
        return this;
    }

    //public CollectionConfig WithInlineMigration<TSourceModel, TTargetModel>
    public InlineCollectionConfig<TSourceModel, TTargetModel> WithInlineMigration<TSourceModel, TTargetModel>
    (
        int? from,
        int to,
        Func<TSourceModel, TTargetModel> mapper,
        Action<FuncMigration<TSourceModel, TTargetModel>>? configure = null
    )
        where TSourceModel : class
        where TTargetModel : class
    {
        var migrationBase = FuncMigration.Create(from, to, mapper);
        configure?.Invoke(migrationBase);
        Migrations.Add(migrationBase);

        //_implicitLatestVersion = to;
        _data.ImplicitLatestVersion = to;

        return new InlineCollectionConfig<TSourceModel, TTargetModel>(this, migrationBase);
    }

    public InlineCollectionConfig<TSourceModel, TTargetModel> WithInlineMigration<TSourceModel, TTargetModel>
    (
        Func<TSourceModel, TTargetModel> mapper,
        Action<FuncMigration<TSourceModel, TTargetModel>>? configure = null
    )
       where TSourceModel : class
       where TTargetModel : class
    {
        int? from = Migrations.Count == 0 ? null : Migrations.Last().To;
        var to = (from ?? 0) + 1;

        var migrationBase = FuncMigration.Create(from, to, mapper);
        configure?.Invoke(migrationBase);
        Migrations.Add(migrationBase);

        //_implicitLatestVersion = to;
        _data.ImplicitLatestVersion = to;

        return new InlineCollectionConfig<TSourceModel, TTargetModel>(this, migrationBase);
    }

    public MigrationRegistry GetRegistry()
    {
        var registry = new MigrationRegistry();

        var hasDefault = Migrations.Any(x => x.From == null);
        var isFirst = true;

        foreach (var migration in Migrations)
        {
            var isDefault = !hasDefault && isFirst;
            registry.RegisterMigration(migration, isDefaultMigration: isDefault);

            isFirst = false;
        }

        return registry;
    }

    public InlineCollectionConfigStarter<TSourceModel> WithMigrationStart<TSourceModel>()
        where TSourceModel : class
    {
        return new InlineCollectionConfigStarter<TSourceModel>(this);
    }
}

public class InlineCollectionConfigStarter<TSourceModel>
    where TSourceModel : class
{
    private readonly CollectionConfig _config;

    public InlineCollectionConfigStarter(CollectionConfig config)
    {
        _config = config;
    }

    public InlineCollectionConfig<TSourceModel, TTargetModel> WithInlineMigration<TTargetModel>(int? from, int to, Func<TSourceModel, TTargetModel> migration)
        where TTargetModel : class
    {
        return _config.WithInlineMigration(from, to, migration);
    }

    public InlineCollectionConfig<TSourceModel, TTargetModel> WithInlineMigration<TTargetModel>(Func<TSourceModel, TTargetModel> migration)
        where TTargetModel : class
    {
        return _config.WithInlineMigration(migration);
    }
}

public interface IStartModel<TSourceModel>
    where TSourceModel : class
{
}

public interface ITargeModel<TTargetModel>
    where TTargetModel : class
{
}

public class InlineCollectionConfig<TSourceModel, TTargetModel>
    : IStartModel<TSourceModel>, ITargeModel<TTargetModel>
    where TSourceModel : class
    where TTargetModel : class
{
    private readonly CollectionConfig _config;
    private FuncMigration<TSourceModel, TTargetModel> _migration;

    public InlineCollectionConfig(CollectionConfig config, FuncMigration<TSourceModel, TTargetModel> migration)
    {
        _config = config;
        _migration = migration;
    }

    // Mirror all methods from CollectionConfig
    public CollectionConfig WithLatestVersion(int version)
    {
        return _config.WithLatestVersion(version);
    }

    public CollectionConfig WithMigrations(IEnumerable<MigrationBase> migrations, bool deduplicate = false)
    {
        return _config.WithMigrations(migrations, deduplicate);
    }

    public CollectionConfig WithMigration<T>()
        where T : MigrationBase, new()
    {
        return _config.WithMigration<T>();
    }

    public InlineCollectionConfig<TSrc, TTarget> WithInlineMigration<TSrc, TTarget>(int? from, int to, Func<TSrc, TTarget> migration, Action<FuncMigration<TSrc, TTarget>>? configure = null)
        where TSrc : class
        where TTarget : class
    {
        return _config.WithInlineMigration(from, to, migration, configure);
    }

    public InlineCollectionConfig<TTargetModel, TNewTarget> WithInlineMigration<TNewTarget>(Func<TTargetModel, TNewTarget> migration)
        where TNewTarget : class
    {
        var from = _migration.To;
        var to = _migration.To + 1;

        return _config.WithInlineMigration(from, to, migration);
    }

    /// <summary>
    /// Creates an inline migration from the source model to the target model.
    /// Assumes the source version to be the previous version of the target version.
    /// </summary>
    /// <typeparam name="TSrc"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="migration"></param>
    /// <returns></returns>
    public InlineCollectionConfig<TSrc, TTarget> WithInlineMigration<TSrc, TTarget>(Func<TSrc, TTarget> migration)
        where TSrc : class
        where TTarget : class
    {
        var from = _migration.To;
        var to = _migration.To + 1;

        return _config.WithInlineMigration(from, to, migration);
    }

    /// <summary>
    /// Overrides the serialization mapper for this migration.
    /// </summary>
    /// <param name="mapper">The serialization mapper to use.</param>
    /// <returns>The current instance of the migration.</returns>
    public InlineCollectionConfig<TSourceModel, TTargetModel> WithSerializationMapper(BsonMapper mapper)
    {
        _migration = _migration.WithSerializationMapper(mapper);
        return this;
    }

    /// <summary>
    /// Overrides the deserialization mapper for this migration.
    /// </summary>
    /// <param name="mapper">The deserialization mapper to use.</param>
    /// <returns>The current instance of the migration.</returns>
    public InlineCollectionConfig<TSourceModel, TTargetModel> WithDeSerializationMapper(BsonMapper mapper)
    {
        _migration = _migration.WithDeSerializationMapper(mapper);
        return this;
    }

    /// <summary>
    /// Overrides the serialization and deserialization mapper for this migration.
    /// </summary>
    public InlineCollectionConfig<TSourceModel, TTargetModel> WithBsonMapper(BsonMapper mapper)
    {
        _migration = _migration.WithBsonMapper(mapper);
        return this;
    }
}