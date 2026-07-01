using CustomerManager.Application.Queries;
using CustomerManager.Application.Validators;
using FluentValidation.TestHelper;

namespace CustomerManager.UnitTests.Application.Validators;

public class GetAllCustomerValidatorTests
{
    private readonly GetAllCustomerValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenQueryIsValid()
    {
        var query = new GetAllCustomerQuery(1, 10, true);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenPaginaIsLessThanOne(int pagina)
    {
        var query = new GetAllCustomerQuery(pagina, 10, true);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagina)
            .WithErrorMessage("A página deve ser maior ou igual a 1.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPaginaIsExactlyOne()
    {
        // Boundary: exactly 1 — the minimum allowed
        var query = new GetAllCustomerQuery(1, 10, true);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Pagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validate_ShouldHaveError_WhenTamanhoPaginaIsZeroOrLess(int tamanho)
    {
        var query = new GetAllCustomerQuery(1, tamanho, true);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TamanhoPagina)
            .WithErrorMessage("O tamanho da página deve ser maior que 0.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenTamanhoPaginaIsExactlyOne()
    {
        // Boundary: exactly 1 — the minimum allowed
        var query = new GetAllCustomerQuery(1, 1, true);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.TamanhoPagina);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(1000)]
    public void Validate_ShouldHaveError_WhenTamanhoPaginaIsGreaterThan100(int tamanho)
    {
        var query = new GetAllCustomerQuery(1, tamanho, true);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TamanhoPagina)
            .WithErrorMessage("O tamanho máximo da página permitido é 100.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenTamanhoPaginaIsExactly100()
    {
        // Boundary: exactly 100 — the maximum allowed
        var query = new GetAllCustomerQuery(1, 100, true);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.TamanhoPagina);
    }

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenQueryUsesDefaultValues()
    {
        var query = new GetAllCustomerQuery();
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenAtivosIsFalse()
    {
        var query = new GetAllCustomerQuery(1, 10, false);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
