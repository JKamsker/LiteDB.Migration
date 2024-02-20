using LiteDB;

namespace LiteDB.Migration.ConsoleTest.Migrations._1_0_2;

internal class ModelB_1_0_2
{
    [BsonId]
    public Guid Id { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public OrderAddress Address { get; set; }

    public ProductInfo ProductInfo { get; set; }
}

internal class ProductInfo
{
    public string ProductName { get; set; }
    public double Price { get; set; }
}

internal class OrderAddress
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }
}

internal class ModelB_Mapper : MigrationBase<_1_0_1.ModelB_1_0_1, ModelB_1_0_2>
{
    public override int? From => 1;
    public override int To => 2;

    public override ModelB_1_0_2 Migrate(_1_0_1.ModelB_1_0_1 model)
    {
        return new ModelB_1_0_2
        {
            Id = model.Id,
            OrderedAt = model.OrderedAt,
            Quantity = model.Quantity,
            Address = new OrderAddress
            {
                Address = model.OrderAddres.Address,
                City = model.OrderAddres.City,
                Region = model.OrderAddres.Region,
                Country = model.OrderAddres.Country,
                PostalCode = model.OrderAddres.PostalCode
            },
            ProductInfo = new ProductInfo
            {
                ProductName = model.ProductName,
                Price = model.Price
            }
        };
    }
}