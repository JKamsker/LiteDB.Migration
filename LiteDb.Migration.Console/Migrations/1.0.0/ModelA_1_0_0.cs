using LiteDB;

namespace LiteDB.Migration.ConsoleTest.Migrations._1_0_0;

internal class ModelA_1_0_0
{
    [BsonId]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public int Age { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }
    public string PostalCode { get; set; }
}