using LiteDB;

namespace LiteDB.Migration.Tests.MigrationContainerTests;

internal class ModelB
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

internal class ModelB_V1
{
    [BsonId]
    public Guid Id { get; set; }

    public DateTime OrderedAt { get; set; }

    public int Quantity { get; set; }

    public OrderAddress Address { get; set; }

    public ProductInfo ProductInfo { get; set; }

    public bool ContentEquals(ModelB_V1 other)
    {
        return OrderedAt == other.OrderedAt && Quantity == other.Quantity
            && Address.ContentEquals(other.Address)
            && ProductInfo.ContentEquals(other.ProductInfo);
    }
}

internal class ProductInfo
{
    public string ProductName { get; set; }
    public double Price { get; set; }

    public bool ContentEquals(ProductInfo other)
    {
        return ProductName == other.ProductName && Price == other.Price;
    }
}

internal class OrderAddress
{
    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public string PostalCode { get; set; }

    public bool ContentEquals(OrderAddress other)
    {
        return Address == other.Address
            && City == other.City
            && Region == other.Region
            && Country == other.Country
            && PostalCode == other.PostalCode;
    }
}