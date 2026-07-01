using CustomerManager.Domain.Entities;
using FluentAssertions;

namespace CustomerManager.UnitTests.Domain.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomer_WhenParametersAreValid()
    {
        // Arrange & Act
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Assert
        customer.CardHolderName.Should().Be("John Doe");
        customer.Cpf.Should().Be("123.456.789-09");
        customer.Status.Should().BeTrue();
        customer.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId_ForEachInstance()
    {
        // Arrange & Act
        var customer1 = new Customer("John Doe", "123.456.789-09", true);
        var customer2 = new Customer("Jane Doe", "987.654.321-00", true);

        // Assert
        customer1.Id.Should().NotBe(customer2.Id);
    }

    [Fact]
    public void Constructor_ShouldCreateInactiveCustomer_WhenStatusIsFalse()
    {
        // Arrange & Act
        var customer = new Customer("John Doe", "123.456.789-09", false);

        // Assert
        customer.Status.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldStoreAllProperties_WhenCalled()
    {
        // Arrange
        const string name = "Alice Wonderland";
        const string cpf = "111.222.333-44";

        // Act
        var customer = new Customer(name, cpf, true);

        // Assert
        customer.CardHolderName.Should().Be(name);
        customer.Cpf.Should().Be(cpf);
    }

    [Fact]
    public void Inativar_ShouldSetStatusToFalse_WhenCustomerIsActive()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Act
        customer.Inativar();

        // Assert
        customer.Status.Should().BeFalse();
    }

    [Fact]
    public void Inativar_ShouldKeepStatusFalse_WhenCustomerIsAlreadyInactive()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", false);

        // Act
        customer.Inativar();

        // Assert
        customer.Status.Should().BeFalse();
    }

    [Fact]
    public void Inativar_ShouldNotChangeOtherProperties_WhenCalled()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);
        var originalId = customer.Id;

        // Act
        customer.Inativar();

        // Assert
        customer.Id.Should().Be(originalId);
        customer.CardHolderName.Should().Be("John Doe");
        customer.Cpf.Should().Be("123.456.789-09");
    }

    [Fact]
    public void Atualizar_ShouldUpdateCardHolderNameAndCpf_WhenParametersAreValid()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Act
        customer.Atualizar("Jane Doe", "987.654.321-00");

        // Assert
        customer.CardHolderName.Should().Be("Jane Doe");
        customer.Cpf.Should().Be("987.654.321-00");
    }

    [Fact]
    public void Atualizar_ShouldNotChangeStatus_WhenUpdatingNameAndCpf()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Act
        customer.Atualizar("Jane Doe", "987.654.321-00");

        // Assert
        customer.Status.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_ShouldNotChangeId_WhenUpdatingNameAndCpf()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);
        var originalId = customer.Id;

        // Act
        customer.Atualizar("Jane Doe", "987.654.321-00");

        // Assert
        customer.Id.Should().Be(originalId);
    }

    [Fact]
    public void Atualizar_ShouldAcceptEmptyStrings_WhenPassedEmptyValues()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Act
        customer.Atualizar(string.Empty, string.Empty);

        // Assert
        customer.CardHolderName.Should().BeEmpty();
        customer.Cpf.Should().BeEmpty();
    }

    [Fact]
    public void Atualizar_ThenInativar_ShouldReflectBothChanges_WhenCalledSequentially()
    {
        // Arrange
        var customer = new Customer("John Doe", "123.456.789-09", true);

        // Act
        customer.Atualizar("Jane Doe", "987.654.321-00");
        customer.Inativar();

        // Assert
        customer.CardHolderName.Should().Be("Jane Doe");
        customer.Cpf.Should().Be("987.654.321-00");
        customer.Status.Should().BeFalse();
    }
}
