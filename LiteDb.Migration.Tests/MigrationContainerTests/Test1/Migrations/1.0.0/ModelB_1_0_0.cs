using LiteDB;

namespace LiteDB.Migration.Tests.Migrations._1_0_0;

internal class ModelB_1_0_0
{
    [BsonId]
    public Guid Id { get; set; }

    public string ProductName { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public double Price { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }
}