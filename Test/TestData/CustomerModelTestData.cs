using Customer.POC.Models;

namespace Test.TestData;

public static class CustomerModelTestData
{
    public static CustomerModel Default =>
        new CustomerModel
        {
            FirstName = "firstName",
            LastName = "lastName",
            DateOfBirth = "20/02/1995",
            Country = "country"
        };
    public static CustomerModel MissingFirstName => 
        new CustomerModel()
        {
            LastName = "lastName",
            DateOfBirth = "20/02/1995",
            Country = "country"
        };
    
    public static CustomerModel MissingLastName =>
        new CustomerModel
        {
            FirstName = "firstName",
            DateOfBirth = "20/02/1995",
            Country = "country"
        };

    public static CustomerModel MissingDateOfBirth =>
        new CustomerModel
        {
            FirstName = "firstName",
            LastName = "lastName",
            Country = "country"
        };
    
    public static CustomerModel MissingCountry =>
        new CustomerModel
        {
            FirstName = "firstName",
            LastName = "lastName",
            DateOfBirth = "20/02/1995",
        };
}