namespace LiteDB.Migration.ConsoleTest.Migrations._1_0_1;

public class MigrationSet_1_0_1 : IMigrationSet
{
    public MigrationSetItem[] Items => new MigrationSetItem[]
    {
        new(nameof(ModelA), new ModelA_Mapper()),
        new(nameof(ModelB), new ModelB_Mapper())
    };
}