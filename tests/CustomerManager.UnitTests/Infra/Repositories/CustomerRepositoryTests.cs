using CustomerManager.Domain.Entities;
using CustomerManager.Infra.Database;
using CustomerManager.Infra.Repositories;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;

namespace CustomerManager.UnitTests.Infra.Repositories;

public class CustomerRepositoryTests
{
    private static CustomerContext CreateContext() => FakeDbContext.Create();

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnTrue_WhenActiveCustomerWithCpfExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithCpf("123.456.789-09").WithStatus(true).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var exists = await repository.ExistsByCpfAsync("123.456.789-09");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnFalse_WhenNoCustormerWithCpfExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        // Act
        var exists = await repository.ExistsByCpfAsync("000.000.000-00");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnFalse_WhenCustomerWithCpfIsInactive()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithCpf("123.456.789-09").WithStatus(false).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var exists = await repository.ExistsByCpfAsync("123.456.789-09");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnTrue_OnlyForActiveCustomer_WhenBothActiveAndInactiveExistForSameCpf()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        var activeCpf = "111.222.333-44";
        var inactiveCpf = "555.666.777-88";

        repository.Add(new CustomerBuilder().WithCpf(activeCpf).WithStatus(true).Build());
        repository.Add(new CustomerBuilder().WithCpf(inactiveCpf).WithStatus(false).Build());
        await repository.SaveChangesAsync();

        // Act
        var activeExists = await repository.ExistsByCpfAsync(activeCpf);
        var inactiveExists = await repository.ExistsByCpfAsync(inactiveCpf);

        // Assert
        activeExists.Should().BeTrue();
        inactiveExists.Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomerByCpfAsync_ShouldReturnCustomer_WhenActiveCustomerWithCpfExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder()
            .WithCpf("123.456.789-09")
            .WithCardHolderName("John Doe")
            .WithStatus(true)
            .Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetCustomerByCpfAsync("123.456.789-09");

        // Assert
        result.Should().NotBeNull();
        result!.Cpf.Should().Be("123.456.789-09");
        result.CardHolderName.Should().Be("John Doe");
        result.Status.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerByCpfAsync_ShouldReturnNull_WhenNoCustomerWithCpfExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetCustomerByCpfAsync("000.000.000-00");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerByCpfAsync_ShouldReturnNull_WhenCustomerWithCpfIsInactive()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithCpf("123.456.789-09").WithStatus(false).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetCustomerByCpfAsync("123.456.789-09");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerByCpfAsync_ShouldReturnCorrectCustomer_WhenMultipleCustomersExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(new CustomerBuilder().WithCpf("111.222.333-44").WithCardHolderName("Alice").Build());
        repository.Add(new CustomerBuilder().WithCpf("555.666.777-88").WithCardHolderName("Bob").Build());
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetCustomerByCpfAsync("555.666.777-88");

        // Assert
        result.Should().NotBeNull();
        result!.CardHolderName.Should().Be("Bob");
        result.Cpf.Should().Be("555.666.777-88");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = CustomerBuilder.Default();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.CardHolderName.Should().Be(customer.CardHolderName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnInactiveCustomer_WhenCustomerExistsButIsInactive()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithStatus(false).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByCpfAsync_ShouldReturnFalse_AfterCustomerIsInactivated()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithCpf("123.456.789-09").WithStatus(true).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        customer.Inativar();
        repository.Update(customer);
        await repository.SaveChangesAsync();

        var exists = await repository.ExistsByCpfAsync("123.456.789-09");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomerByCpfAsync_ShouldReturnNull_AfterCustomerIsInactivated()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder().WithCpf("123.456.789-09").WithStatus(true).Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        customer.Inativar();
        repository.Update(customer);
        await repository.SaveChangesAsync();

        var result = await repository.GetCustomerByCpfAsync("123.456.789-09");

        // Assert
        result.Should().BeNull();
    }
}
