using LiteDB;

namespace LiteDB.Migration.Tests.MigrationContainerTests;

internal class Seeder
{
    public static IEnumerable<ModelA> SeeedModelA()
    {
        return new[]
        {
            new ModelA
            {
                Name = "John",
                Age = 30,
                City = "New York",
                Region = "New York",
                Country = "USA",
                PostalCode = "10001"
            },
            new ModelA
            {
                Name = "Jane",
                Age = 25,
                City = "Los Angeles",
                Region = "California",
                Country = "USA",
                PostalCode = "90001"
            },
            new ModelA
            {
                Name = "Doe",
                Age = 35,
                City = "San Francisco",
                Region = "California",
                Country = "USA",
                PostalCode = "94016"
            },
            new ModelA
            {
                Name = "Doe",
                Age = 35,
                City = "San Francisco",
                Region = "California",
                Country = "USA",
                PostalCode = "94016"
            }
        };
    }

    public static IEnumerable<ModelB> SeeedModelB()
    {
        return new[]
        {
            new ModelB
            {
                ProductName = "Laptop",
                OrderedAt = new DateTime(2024, 1, 15),
                Quantity = 1,
                Price = 1200.50,
                Address = "123 Tech Road",
                City = "Techville",
                Region = "Tech Region",
                Country = "Techland",
                PostalCode = "12345"
            },
            new ModelB
            {
                ProductName = "Smartphone",
                OrderedAt = new DateTime(2024, 1, 20),
                Quantity = 2,
                Price = 800.00,
                Address = "456 Gadget St.",
                City = "Gadgetville",
                Region = "Gadget Region",
                Country = "Gadgetland",
                PostalCode = "67890"
            },
            new ModelB
            {
                ProductName = "Tablet",
                OrderedAt = new DateTime(2024, 1, 25),
                Quantity = 3,
                Price = 600.75,
                Address = "789 Pad Ave.",
                City = "Tabletville",
                Region = "Tablet Region",
                Country = "Tabletland",
                PostalCode = "11223"
            },
            new ModelB
            {
                ProductName = "Wireless Earbuds",
                OrderedAt = new DateTime(2024, 2, 5),
                Quantity = 4,
                Price = 200.99,
                Address = "101 Sound Lane",
                City = "Auraltown",
                Region = "Audio Region",
                Country = "Audioland",
                PostalCode = "44556"
            },
            new ModelB
            {
                ProductName = "Smartwatch",
                OrderedAt = new DateTime(2024, 2, 10),
                Quantity = 2,
                Price = 350.50,
                Address = "202 Timepiece Blvd.",
                City = "Chronoville",
                Region = "Chrono Region",
                Country = "Chronoland",
                PostalCode = "77889"
            },
            new ModelB
            {
                ProductName = "Gaming Console",
                OrderedAt = new DateTime(2024, 2, 15),
                Quantity = 1,
                Price = 500.00,
                Address = "303 Gamer's Gate",
                City = "Gamestown",
                Region = "Gaming Region",
                Country = "Gameland",
                PostalCode = "99001"
            }
        };
    }

    public static IEnumerable<ModelB_V1> GetModelB_Expectations()
    {
        return new[]
        {
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 1, 15),
                Quantity = 1,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Laptop",
                    Price = 1200.50
                },
                Address = new OrderAddress
                {
                    Address = "123 Tech Road",
                    City = "Techville",
                    Region = "Tech Region",
                    Country = "Techland",
                    PostalCode = "12345"
                }
            },
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 1, 20),
                Quantity = 2,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Smartphone",
                    Price = 800.00
                },
                Address = new OrderAddress
                {
                    Address = "456 Gadget St.",
                    City = "Gadgetville",
                    Region = "Gadget Region",
                    Country = "Gadgetland",
                    PostalCode = "67890"
                }
            },
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 1, 25),
                Quantity = 3,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Tablet",
                    Price = 600.75
                },
                Address = new OrderAddress
                {
                    Address = "789 Pad Ave.",
                    City = "Tabletville",
                    Region = "Tablet Region",
                    Country = "Tabletland",
                    PostalCode = "11223"
                }
            },
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 2, 5),
                Quantity = 4,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Wireless Earbuds",
                    Price = 200.99
                },
                Address = new OrderAddress
                {
                    Address = "101 Sound Lane",
                    City = "Auraltown",
                    Region = "Audio Region",
                    Country = "Audioland",
                    PostalCode = "44556"
                }
            },
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 2, 10),
                Quantity = 2,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Smartwatch",
                    Price = 350.50
                },
                Address = new OrderAddress
                {
                    Address = "202 Timepiece Blvd.",
                    City = "Chronoville",
                    Region = "Chrono Region",
                    Country = "Chronoland",
                    PostalCode = "77889"
                }
            },
            new ModelB_V1
            {
                OrderedAt = new DateTime(2024, 2, 15),
                Quantity = 1,
                ProductInfo = new ProductInfo
                {
                    ProductName = "Gaming Console",
                    Price = 500.00
                },
                Address = new OrderAddress
                {
                    Address = "303 Gamer's Gate",
                    City = "Gamestown",
                    Region = "Gaming Region",
                    Country = "Gameland",
                    PostalCode = "99001"
                }
            }
        };
    }

    public static void SeedData(ILiteCollection<ModelA> collection)
    {
        var data = SeeedModelA();
        collection.InsertBulk(data);
    }

    public static void SeedData(ILiteCollection<ModelB> collection)
    {
        var data = SeeedModelB();
        collection.InsertBulk(data);
    }
}