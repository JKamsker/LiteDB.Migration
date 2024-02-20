using LiteDB;

namespace LiteDB.Migration.Tests.MigrationContainerTests;

public class ModelA
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

public class ModelA_V1
{
    public Guid Id { get; set; }
    public Person Person { get; set; }
    public Address Address { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}