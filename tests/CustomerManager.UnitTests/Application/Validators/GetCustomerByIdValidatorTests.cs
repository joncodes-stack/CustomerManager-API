using CustomerManager.Application.Queries;
using CustomerManager.Application.Validators;
using FluentValidation.TestHelper;

namespace CustomerManager.UnitTests.Application.Validators;

public class GetCustomerByIdValidatorTests
{
    private readonly GetCustomerByIdValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenQueryIsValid()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsGuidEmpty()
    {
        var query = new GetCustomerByIdQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsDefault()
    {
        var query = new GetCustomerByIdQuery(default);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenIdIsValidGuid()
    {
        var query = new GetCustomerByIdQuery(Guid.Parse("6f9619ff-8b86-d011-b42d-00cf4fc964ff"));
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenIdIsNewGuid()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
