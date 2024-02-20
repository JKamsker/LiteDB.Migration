namespace LiteDB.Migration.Container;

internal class MigrationHistory
{
    public Guid Id { get; set; }
    public string CollectionName { get; set; }
    public int Version { get; set; }

    public DateTime AppliedOn { get; set; }
}
