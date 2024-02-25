namespace LiteDB.Migration.ConsoleTest.Migrations._1_0_1;

internal class ModelA_1_0_1
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public Address Address { get; set; }
}

internal class Address
{
    public string City { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}

internal class ModelA_Mapper : MigrationBase<_1_0_0.ModelA_1_0_0, ModelA_1_0_1>
{
    public override int? From => 0;
    public override int To => 1;

    public override ModelA_1_0_1 Migrate(_1_0_0.ModelA_1_0_0 model)
    {
        return new ModelA_1_0_1
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            Address = new Address
            {
                City = model.City,
                Region = model.Region,
                Country = model.Country,
                PostalCode = model.PostalCode
            }
        };
    }
}