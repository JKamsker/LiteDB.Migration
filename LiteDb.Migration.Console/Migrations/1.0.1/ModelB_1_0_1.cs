using LiteDB;

namespace LiteDB.Migration.ConsoleTest.Migrations._1_0_1;

internal class ModelB_1_0_1
{
    [BsonId]
    public Guid Id { get; set; }

    public string ProductName { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public double Price { get; set; }

    public OrderAddress OrderAddres { get; set; }
}

internal class OrderAddress
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }
}

internal class ModelB_Mapper : MigrationBase<_1_0_0.ModelB_1_0_0, ModelB_1_0_1>
{
    public override int? From => 0;
    public override int To => 1;

    public override ModelB_1_0_1 Migrate(_1_0_0.ModelB_1_0_0 model)
    {
        return new ModelB_1_0_1
        {
            Id = model.Id,
            ProductName = model.ProductName,
            OrderedAt = model.OrderedAt,
            Quantity = model.Quantity,
            Price = model.Price,
            OrderAddres = new OrderAddress
            {
                Address = model.Address,
                City = model.City,
                Region = model.Region,
                Country = model.Country,
                PostalCode = model.PostalCode
            }
        };
    }
}