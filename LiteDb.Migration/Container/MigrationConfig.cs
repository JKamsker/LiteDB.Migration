namespace LiteDB.Migration.Container;

public class MigrationConfig
{
    private Dictionary<string, CollectionConfig> _collections = new Dictionary<string, CollectionConfig>();

    public IEnumerable<KeyValuePair<string, CollectionConfig>> Collections => _collections;

    public bool ShouldUseMigrationHistory { get; private set; } = true;

    private List<MigrationSetItem> _migrationSets = new List<MigrationSetItem>();

    public MigrationConfig()
    {
    }

    public MigrationConfig Collection(string collectionName, Action<CollectionConfig> configure)
    {
        //var collectionConfig = new CollectionConfig();
        var collectionConfig = GetOrCreateConfig(collectionName);
        configure(collectionConfig);
        _collections.Add(collectionName, collectionConfig);
        return this;
    }

    public MigrationConfig Collection<T>(string collectionName, Action<CollectionConfig<T>>? configure = null)
        where T : class
    {
        var typeName = typeof(T).Name;
        var sets = _migrationSets.Where(x => string.Equals(x.Name, typeName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Migration);

        var collectionConfig = GetOrCreateConfig(collectionName);
        collectionConfig.WithMigrations(sets, true);

        configure?.Invoke(new CollectionConfig<T>(collectionConfig));
        return this;
    }

    private CollectionConfig GetOrCreateConfig(string collectionName)
    {
        if (_collections.TryGetValue(collectionName, out var config))
        {
            return config;
        }

        var collectionConfig = new CollectionConfig();
        _collections.Add(collectionName, collectionConfig);
        return collectionConfig;
    }

    public MigrationConfig WithSet<T>()
        where T : IMigrationSet, new()
    {
        return WithSet(new T());
    }

    public MigrationConfig WithSet(IMigrationSet set)
    {
        _migrationSets.AddRange(set.Items);
        return this;
    }

    public MigrationConfig UseMigrationHistory(bool useHistory = true)
    {
        ShouldUseMigrationHistory = useHistory;
        return this;
    }
}