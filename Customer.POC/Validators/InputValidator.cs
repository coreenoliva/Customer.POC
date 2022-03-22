using Customer.POC.Models;
using FluentValidation;

namespace Customer.POC.Validators;

public class InputValidator : AbstractValidator<CustomerModel>
{
    public InputValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        // todo add additional validation for date of birth (e.g. format)
    }
}