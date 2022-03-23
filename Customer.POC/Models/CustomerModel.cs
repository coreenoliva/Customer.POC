using System;

namespace Customer.POC.Models;

public class CustomerModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DateOfBirth { get; set; }
    public string Country { get; set; }
}

public class CustomerModelCosmos : CustomerModel
{
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
    public string Id { get; set; }
}