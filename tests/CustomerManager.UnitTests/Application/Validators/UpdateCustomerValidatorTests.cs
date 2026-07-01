using CustomerManager.Application.Commands;
using CustomerManager.Application.Validators;
using FluentValidation.TestHelper;

namespace CustomerManager.UnitTests.Application.Validators;

public class UpdateCustomerValidatorTests
{
    private readonly UpdateCustomerValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane Doe", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldHaveError_WhenCardHolderNameIsEmpty(string? name)
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), name!, "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName)
            .WithErrorMessage("O nome é obrigatório.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCardHolderNameIsTooShort()
    {
        // Boundary: length 1 — just below minimum of 2
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Z", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName)
            .WithErrorMessage("O nome deve ter pelo menos 2 caracteres.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCardHolderNameHasExactlyMinLength()
    {
        // Boundary: length 2 — exactly the minimum
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "AB", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CardHolderName);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCardHolderNameIsTooLong()
    {
        // Boundary: length 101 — just above maximum of 100
        var name = new string('Z', 101);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), name, "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName)
            .WithErrorMessage("O nome deve ter no máximo 100 caracteres.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCardHolderNameHasExactlyMaxLength()
    {
        // Boundary: length 100 — exactly the maximum
        var name = new string('A', 100);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), name, "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CardHolderName);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCardHolderNameContainsNumbers()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane456", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName)
            .WithErrorMessage("O nome deve conter apenas letras.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCardHolderNameContainsSpecialCharacters()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane#Doe", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName)
            .WithErrorMessage("O nome deve conter apenas letras.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCardHolderNameHasAccentedCharacters()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Márcia Oliveira", "987.654.321-00");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CardHolderName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldHaveError_WhenCpfIsEmpty(string? cpf)
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane Doe", cpf!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cpf)
            .WithErrorMessage("O CPF é obrigatório.");
    }

    [Theory]
    [InlineData("98765432100")]
    [InlineData("987.654.321-0")]
    [InlineData("abc.def.ghi-jk")]
    public void Validate_ShouldHaveError_WhenCpfIsInvalidFormat(string cpf)
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane Doe", cpf);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cpf)
            .WithErrorMessage("O CPF deve estar no formato XXX.XXX.XXX-XX.");
    }

    [Theory]
    [InlineData("987.654.321-00")]
    [InlineData("123.456.789-09")]
    public void Validate_ShouldNotHaveError_WhenCpfIsValidFormat(string cpf)
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Jane Doe", cpf);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Cpf);
    }
}
