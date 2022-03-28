using System;
using Newtonsoft.Json;

namespace Customer.POC.Models;

public class CustomerModel
{
    [JsonProperty("firstName")] public string FirstName { get; set; }

    [JsonProperty("lastName")] public string LastName { get; set; }

    [JsonProperty("dateOfBirth")] public string DateOfBirth { get; set; }

    [JsonProperty("country")] public string Country { get; set; }
}

public class CustomerModelCosmos : CustomerModel
{
    [JsonProperty("id")] public string Id { get; set; }

    public CustomerModelCosmos CreateCustomer(CustomerModel customerModel)
    {
        return new CustomerModelCosmos
        {
            FirstName = customerModel.FirstName,
            LastName = customerModel.LastName,
            DateOfBirth = customerModel.DateOfBirth,
            Country = customerModel.Country,
            Id = Guid.NewGuid().ToString()
        };
    }
}