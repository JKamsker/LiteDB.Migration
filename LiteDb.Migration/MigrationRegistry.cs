using System.Runtime.CompilerServices;

using LiteDB.Migration.Helpers;

using LiteDB;

using Microsoft.Win32;
using System;

[assembly: InternalsVisibleTo("LiteDB.Migration.Tests")]

namespace LiteDB.Migration;

public class MigrationRegistry
{
    private List<MigrationBase> _migrations = new List<MigrationBase>();

    public void RegisterMigration(int? from, int to, Func<BsonValue, BsonValue> migration)
    {
        _migrations.Add(new FuncMigration(from, to, migration));
    }

    public void RegisterMigration(MigrationBase migration, int? from, int to)
    {
        _migrations.Add(new FuncMigration(from, to, migration.ApplyMigration));
    }

    /// <summary>
    /// Registers a migration and sets it as the default migration.
    /// </summary>
    /// <param name="migration">The migration to register.</param>
    /// <param name="isDefaultMigration">If true, the migration will be set as the default migration.</param>
    public void RegisterMigration(MigrationBase migration, bool isDefaultMigration = false)
    {
        RegisterMigration(migration);

        if (isDefaultMigration)
        {
            RegisterMigration(migration, null, migration.To);

            //if (migration is IObjectMigratable migratable)
            //{
            //    RegisterMigration(new ObjectMigratableWrapper(migratable, "", migration.To));
            //}
            //else
            //{
            //    RegisterMigration(migration, "", migration.To);
            //}
        }
    }

    public T RegisterMigration<T>(bool isDefaultMigration = false)
        where T : MigrationBase, new()
    {
        var migration = new T();
        RegisterMigration(migration, isDefaultMigration);
        return migration;
    }

    private MigrationBase RegisterMigration(MigrationBase migration)
    {
        var exists = _migrations.Any(x => x.From == migration.From && x.To == migration.To);
        if (exists)
        {
            throw new InvalidOperationException($"Migration from {migration.From} to {migration.To} already exists.");
        }

        _migrations.Add(migration);
        return migration;
    }

    public MigrationBase? GetMigration(int? from, int to)
    {
        if (to < 0)
        {
            throw new ArgumentException("Target Version cannot be less than 0.", nameof(to));
        }

        // to must be greater than from
        if (from.HasValue && from.Value >= to)
        {
            throw new ArgumentException("Target Version must be greater than the source version.", nameof(to));
        }

        var result = GetMigrationInternal(from, to);
        if (result == null)
        {
            result = GetMigrationPath(from, to);
        }
        return result;
    }

    private MigrationBase? GetMigrationInternal(int? from, int to)
    {
        return _migrations.FirstOrDefault(x =>
        {
            return x.From == from && x.To == to;
        });
    }

    // Internal for testing
    internal FuncMigration GetMigrationPath(int? from, int to)
    {
        var path = FindMigrationPath(from, to);

        Func<BsonValue, BsonValue> result = bson =>
        {
            var current = bson;
            foreach (var migration in path)
            {
                current = migration.ApplyMigration(current);
            }

            return current;
        };

        var mig = new FuncMigration(from, to, result);
        RegisterMigration(mig);
        return mig;
    }

    // Internal for testing
    internal IEnumerable<MigrationBase> FindMigrationPath(int? from, int to)
    {
        var path = new List<MigrationBase>();
        var currentVersion = from;

        while (currentVersion != to)
        {
            //var nextStep = string.IsNullOrEmpty(currentVersion)
            //    ? _migrations.FirstOrDefault(m => string.IsNullOrEmpty(m.From))
            //    : _migrations.FirstOrDefault(m => m.From == currentVersion);

            var nextStep = _migrations.FirstOrDefault(m => m.From == currentVersion);

            if (nextStep == null)
            {
                throw new InvalidOperationException($"No migration path found from {from} to {to}.");
            }

            path.Add(nextStep);
            currentVersion = nextStep.To;
        }

        return path;
    }

    public void ApplyMigrations(ILiteCollection<BsonDocument> collection, int version, int? defaultInferVersion = null)
    {
        //if (string.IsNullOrEmpty(version))
        //{
        //    throw new ArgumentException("Target Version cannot be null or empty.", nameof(version));
        //}

        if (version < 0)
        {
            throw new ArgumentException("Target Version cannot be less than 0.", nameof(version));
        }

        var documents = collection.FindAll();
        if (false)
        {
            foreach (var document in documents)
            {
                //var documentId = document["_id"];
                //var documentVersion = document["__version"].AsInt32;
                //var documentVersion = (int?)document["__version"].RawValue;
                var documentVersion = ConvertEx.ToNullableInt32(document[Constants.VersionFieldName].RawValue);

                if (documentVersion == version)
                {
                    continue;
                }

                var migration = GetMigration(documentVersion, version);
                if (migration != null)
                {
                    var newDocument = migration.ApplyMigration(document);
                    newDocument[Constants.VersionFieldName] = version;
                    //collection.Update(documentId, (BsonDocument)newDocument);
                    collection.Update((BsonDocument)newDocument);
                }
            }
        }
        else
        {
            // Try bulk
            var newDocuments = new List<BsonDocument>();
            foreach (var document in documents)
            {
                var documentVersion = ConvertEx.ToNullableInt32(document[Constants.VersionFieldName].RawValue) ?? defaultInferVersion;
                if (documentVersion == version)
                {
                    continue;
                }

                var migration = GetMigration(documentVersion, version);
                if (migration != null)
                {
                    var newDocument = migration.ApplyMigration(document);
                    newDocument[Constants.VersionFieldName] = version;
                    newDocuments.Add((BsonDocument)newDocument);

                    if (newDocuments.Count > 1000)
                    {
                        collection.Update(newDocuments);
                        newDocuments.Clear();
                    }
                }
            }

            if (newDocuments.Count > 0)
            {
                collection.Update(newDocuments);
            }
        }
    }
}

public class FuncMigration : MigrationBase
{
    public override int? From { get; }
    public override int To { get; }

    private Func<BsonValue, BsonValue> _applyMigration;

    public override BsonValue ApplyMigration(BsonValue document)
    {
        return _applyMigration(document);
    }

    public FuncMigration(int? from, int to, Func<BsonValue, BsonValue> applyMigration)
    {
        From = from;
        To = to;
        _applyMigration = applyMigration;
    }

    public static FuncMigration<TFrom, TTo> Create<TFrom, TTo>(int? from, int to, Func<TFrom, TTo> migration)
    {
        //Func<BsonValue, BsonValue> mapper = (bson) =>
        //{
        //    var from = BsonMapper.Global.Deserialize<TFrom>(bson);
        //    var to = migration(from);
        //    return BsonMapper.Global.Serialize(to);
        //};

        return new FuncMigration<TFrom, TTo>(from, to, migration);
    }
}

public class FuncMigration<TFrom, TTo> : MigrationBase<TFrom, TTo>
{
    public override int? From { get; }
    public override int To { get; }

    private Func<TFrom, TTo> _migration;

    private BsonMapper? _serializationMapperOverride;
    private BsonMapper? _deSerializationMapperOverride;

    public override BsonMapper SerializationMapper => _serializationMapperOverride ?? base.SerializationMapper;
    public override BsonMapper DeSerializationMapper => _deSerializationMapperOverride ?? base.DeSerializationMapper;

    public FuncMigration(int? from, int to, Func<TFrom, TTo> migration)
    {
        From = from;
        To = to;
        _migration = migration;
    }

    public override TTo Migrate(TFrom document)
    {
        return _migration(document);
    }

    /// <summary>
    /// Overrides the serialization mapper for this migration.
    /// </summary>
    /// <param name="mapper">The serialization mapper to use.</param>
    /// <returns>The current instance of the migration.</returns>
    public FuncMigration<TFrom, TTo> WithSerializationMapper(BsonMapper mapper)
    {
        _serializationMapperOverride = mapper;
        return this;
    }

    /// <summary>
    /// Overrides the deserialization mapper for this migration.
    /// </summary>
    /// <param name="mapper">The deserialization mapper to use.</param>
    /// <returns>The current instance of the migration.</returns>
    public FuncMigration<TFrom, TTo> WithDeSerializationMapper(BsonMapper mapper)
    {
        _deSerializationMapperOverride = mapper;
        return this;
    }

    /// <summary>
    /// Overrides the serialization and deserialization mapper for this migration.
    /// </summary>
    public FuncMigration<TFrom, TTo> WithBsonMapper(BsonMapper mapper)
    {
        _serializationMapperOverride = mapper;
        _deSerializationMapperOverride = mapper;
        return this;
    }
}

public abstract class MigrationBase
{
    public abstract int? From { get; }
    public abstract int To { get; }

    public abstract BsonValue ApplyMigration(BsonValue document);
}

public abstract class MigrationBase<TFrom, TTo> : MigrationBase//, IObjectMigratable
{
    public abstract TTo Migrate(TFrom document);

    public override BsonValue ApplyMigration(BsonValue document)
    {
        var id = document["_id"];
        TFrom? from = Deserialize(document);
        var to = Migrate(from);
        var serialized = Serialize(to);
        serialized["_id"] = id; // Bug with anonymous types is nulling out the _id field
        return serialized;
    }

    public virtual BsonMapper SerializationMapper => BsonMapper.Global;
    public virtual BsonMapper DeSerializationMapper => BsonMapper.Global;

    public virtual BsonValue Serialize(TTo? to) => SerializationMapper.Serialize(to);

    public virtual TFrom? Deserialize(BsonValue document) => DeSerializationMapper.Deserialize<TFrom>(document);
}

public static class MigrationExtensions
{
    //public static void RegisterMigratable<T>(this BsonMapper mapper, MigrationRegistry registry, int version)
    //{
    //    mapper.RegisterType<T>
    //    (
    //        serialize: (model) =>
    //        {
    //            var result = BsonMapper.Global.Serialize(model);
    //            result["__version"] = version;
    //            return result;
    //        },
    //        deserialize: (bson) =>
    //        {
    //            //var dbVersion = bson["__version"].AsInt32;
    //            //var dbVersion = (int?)bson["__version"].RawValue;
    //            var dbVersion = ConvertEx.ToNullableInt32(bson["__version"].RawValue);
    //            if (dbVersion != version)
    //            {
    //                var migration = registry.GetMigration(dbVersion, version);
    //                if (migration != null)
    //                {
    //                    var newValue = migration.ApplyMigration(bson);
    //                    return BsonMapper.Global.Deserialize<T>(newValue);
    //                }
    //            }

    //            return BsonMapper.Global.Deserialize<T>(bson);
    //        }
    //    );
    //}

    public static void RegisterMigratable<T>(this BsonMapper mapper, MigrationRegistry registry, int version)
    {
        var mapperClone = BsonMapperCloner.Clone(mapper);
        mapper.RegisterType<T>
        (
            serialize: (model) =>
            {
                var result = mapperClone.Serialize(model);
                result[Constants.VersionFieldName] = version;
                return result;
            },
            deserialize: (bson) =>
            {
                var dbVersion = ConvertEx.ToNullableInt32(bson[Constants.VersionFieldName].RawValue);
                if (dbVersion != version)
                {
                    var migration = registry.GetMigration(dbVersion, version);
                    if (migration != null)
                    {
                        var newValue = migration.ApplyMigration(bson);
                        return mapperClone.Deserialize<T>(newValue);
                    }
                }

                return mapperClone.Deserialize<T>(bson);
            }
        );
    }
}