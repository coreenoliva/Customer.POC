using Customer.POC.Validators;
using FluentValidation.TestHelper;
using Test.TestData;
using Xunit;

namespace Test;

public class InputValidatorTests
{
    [Fact]
    public void Validate_FirstNameNotSupplied_ReturnsInvalid()
    {
        // Arrange
        var validator = new InputValidator();
        var request = CustomerModelTestData.MissingFirstName;

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_LastNameNotSupplied_ReturnsInvalid()
    {
        // Arrange
        var validator = new InputValidator();
        var request = CustomerModelTestData.MissingLastName;

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_DateOfBirthNotSupplied_ReturnsInvalid()
    {
        // Arrange
        var validator = new InputValidator();
        var request = CustomerModelTestData.MissingDateOfBirth;

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_CountryNotSupplied_ReturnsInvalid()
    {
        // Arrange
        var validator = new InputValidator();
        var request = CustomerModelTestData.MissingCountry;

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }
}