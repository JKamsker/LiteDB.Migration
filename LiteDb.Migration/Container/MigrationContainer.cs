using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB.Migration.Helpers;
using LiteDB.Migration.Container;

namespace LiteDB.Migration;

public class MigrationContainer
{
    private readonly Action<MigrationConfig>? _config;

    public MigrationContainer()
    {
    }

    public MigrationContainer(Action<MigrationConfig>? config)
    {
        _config = config;
    }

    public virtual void Configure(MigrationConfig config)
    {
        _config?.Invoke(config);
    }

    public virtual void Apply(LiteDatabase database)
    {
        var config = new MigrationConfig();
        Configure(config);

        if (config.ShouldUseMigrationHistory)
        {
            ApplyWithHistory(database, config);
        }
        else
        {
            ApplyWithoutHistory(database, config);
        }
    }

    private void ApplyWithHistory(LiteDatabase database, MigrationConfig config)
    {
        var historyCollection = database.GetCollection<MigrationHistory>("__migration_history");

        foreach (var (key, value) in config.Collections)
        {
            var latestVersion = value.LatestVersion
                ?? throw new InvalidOperationException($"No latest version defined for collection {key}");

            if (!database.CollectionExists(key))
            {
                // No migration needed, but we still need to insert the current version into the history
                historyCollection.Insert(new MigrationHistory
                {
                    CollectionName = key,
                    Version = latestVersion,
                    AppliedOn = DateTime.UtcNow
                });
                continue;
            }

            var registry = value.GetRegistry();

            var collection = database.GetCollection(key);
            var history = historyCollection.Find(x => x.CollectionName == key).ToList();

            //var latestVersion = value.LatestVersion
            //    ?? throw new InvalidOperationException($"No latest version defined for collection {key}");

            if (history.Any(x => x.Version == latestVersion))
            {
                // Already migrated
                continue;
            }

            var lastMigration = history
                .Where(x => x.CollectionName == key)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            registry.ApplyMigrations(collection, latestVersion, lastMigration?.Version);
            historyCollection.Insert(new MigrationHistory
            {
                CollectionName = key,
                Version = latestVersion,
                AppliedOn = DateTime.UtcNow
            });
        }
    }

    private void ApplyWithoutHistory(LiteDatabase database, MigrationConfig config)
    {
        foreach (var (key, value) in config.Collections)
        {
            if (!database.CollectionExists(key))
            {
                // No migration needed
                continue;
            }

            var latestVersion = value.LatestVersion
                ?? throw new InvalidOperationException($"No latest version defined for collection {key}");

            var registry = value.GetRegistry();

            var collection = database.GetCollection(key);
            registry.ApplyMigrations(collection, latestVersion);
        }
    }
}