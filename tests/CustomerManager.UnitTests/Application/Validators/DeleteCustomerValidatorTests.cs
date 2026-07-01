using CustomerManager.Application.Commands;
using CustomerManager.Application.Validators;
using FluentValidation.TestHelper;

namespace CustomerManager.UnitTests.Application.Validators;

public class DeleteCustomerValidatorTests
{
    private readonly DeleteCustomerValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var command = new DeleteCustomerCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new DeleteCustomerCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O Id do usuário é obrigatório e deve ser um identificador válido.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsDefault()
    {
        var command = new DeleteCustomerCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenIdIsValidGuid()
    {
        var command = new DeleteCustomerCommand(Guid.Parse("6f9619ff-8b86-d011-b42d-00cf4fc964ff"));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
