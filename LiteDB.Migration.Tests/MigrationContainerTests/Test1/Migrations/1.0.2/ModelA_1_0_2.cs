using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Migration.Tests.Migrations._1_0_2;

internal class ModelA_1_0_2
{
    [BsonId]
    public Guid Id { get; set; }

    public Person Person { get; set; }
    public Address Address { get; set; }

    internal bool ContentEquals(MigrationContainerTests.ModelA_V1 item)
    {
        return 
               item.Person.Name == Person.Name &&
               item.Person.Age == Person.Age &&
               item.Address.City == Address.City &&
               item.Address.Region == Address.Region &&
               item.Address.Country == Address.Country &&
               item.Address.PostalCode == Address.PostalCode;
    }
}

internal class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

internal class Address
{
    public string City { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}

internal class ModelA_Mapper : MigrationBase<_1_0_1.ModelA_1_0_1, ModelA_1_0_2>
{
    public override int? From => 1;
    public override int To => 2;

    public override ModelA_1_0_2 Migrate(_1_0_1.ModelA_1_0_1 document)
    {
        return new ModelA_1_0_2
        {
            Id = document.Id,
            Person = new Person
            {
                Name = document.Name,
                Age = document.Age
            },
            Address = new Address
            {
                City = document.Address.City,
                Region = document.Address.Region,
                Country = document.Address.Country,
                PostalCode = document.Address.PostalCode
            }
        };
    }
}