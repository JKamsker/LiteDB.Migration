using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration;

public interface IMigrationSet
{
    MigrationSetItem[] Items { get; }
}

public abstract class MigrationSet : IMigrationSet
{
    public abstract MigrationSetItem[] Items { get; }

    public static MigrationSetItem Create<TModel, TSourceModel, TTargetModel>(int? from, int to, Func<TSourceModel, TTargetModel> migration)
        where TModel : class
        where TSourceModel : class
        where TTargetModel : class
    {
        return MigrationSetItem.Create<TModel, TSourceModel, TTargetModel>(from, to, migration);
    }


    public static MigrationSetItem Create<TModel>(MigrationBase migration)
      where TModel : class
    {
        return MigrationSetItem.Create<TModel>(migration);
    }


    public static MigrationSetItem Create<TModel, TMigrationBase>()
        where TModel : class
        where TMigrationBase : MigrationBase
    {
        return MigrationSetItem.Create<TModel, TMigrationBase>();
    }
}


public class MigrationSetItem
{
    public string Name { get; }
    public MigrationBase Migration { get; }

    public MigrationSetItem(string name, MigrationBase migration)
    {
        Name = name;
        Migration = migration;
    }

    public static MigrationSetItem<TModel, TSourceModel, TTargetModel> Create<TModel, TSourceModel, TTargetModel>(int? from, int to, Func<TSourceModel, TTargetModel> migration)
        where TModel : class
        where TSourceModel : class
        where TTargetModel : class
    {
        return new MigrationSetItem<TModel, TSourceModel, TTargetModel>(from, to, migration);
    }

    public static MigrationSetItem Create<TModel, TSourceModel, TTargetModel>(MigrationBase<TSourceModel, TTargetModel> migration)
        where TModel : class
        where TSourceModel : class
        where TTargetModel : class
    {
        var name = typeof(TModel).Name;
        return new MigrationSetItem(name, migration);
    }

    public static MigrationSetItem Create<TModel>(MigrationBase migration)
        where TModel : class
    {
        var name = typeof(TModel).Name;
        return new MigrationSetItem(name, migration);
    }


    public static MigrationSetItem Create<TModel, TMigrationBase>()
        where TModel : class
        where TMigrationBase : MigrationBase
    {
        var name = typeof(TModel).Name;
        var migration = (MigrationBase)Activator.CreateInstance(typeof(TMigrationBase));
        return new MigrationSetItem(name, migration);
    }
}

public class MigrationSetItem<TModel, TSourceModel, TTargetModel> : MigrationSetItem
    where TModel : class
    where TSourceModel : class
    where TTargetModel : class
{
    public MigrationSetItem(int? from, int to, Func<TSourceModel, TTargetModel> migration)
        : base(typeof(TModel).Name, CreateMigration(from, to, migration))
    {
    }

    private static MigrationBase CreateMigration(int? from, int to, Func<TSourceModel, TTargetModel> migration)
    {
        var mig = FuncMigration.Create(from, to, migration);
        return mig;
    }
}