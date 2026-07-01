using CustomerManager.Domain.Entities;
using CustomerManager.Infra.Database;
using CustomerManager.Infra.Repositories;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;

namespace CustomerManager.UnitTests.Infra.Repositories;

public class BaseRepositoryTests
{
    private static CustomerContext CreateContext() => FakeDbContext.Create();

    [Fact]
    public async Task Add_ShouldPersistEntity_WhenEntityIsValid()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = CustomerBuilder.Default();

        // Act
        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Assert
        var result = await repository.GetByIdAsync(customer.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Add_ShouldPersistMultipleEntities_WhenCalledForEach()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer1 = CustomerBuilder.Default();
        var customer2 = CustomerBuilder.Default();

        // Act
        repository.Add(customer1);
        repository.Add(customer2);
        await repository.SaveChangesAsync();

        // Assert
        var result1 = await repository.GetByIdAsync(customer1.Id);
        var result2 = await repository.GetByIdAsync(customer2.Id);
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ShouldModifyEntity_WhenEntityExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = CustomerBuilder.Default();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        customer.Atualizar("Updated Name", "999.888.777-66");
        repository.Update(customer);
        await repository.SaveChangesAsync();

        // Assert
        var result = await repository.GetByIdAsync(customer.Id);
        result!.CardHolderName.Should().Be("Updated Name");
        result.Cpf.Should().Be("999.888.777-66");
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity_WhenEntityExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = CustomerBuilder.Default();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        repository.Delete(customer);
        await repository.SaveChangesAsync();

        // Assert
        var result = await repository.GetByIdAsync(customer.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var customer = new CustomerBuilder()
            .WithCardHolderName("Alice")
            .WithCpf("111.222.333-44")
            .Build();

        repository.Add(customer);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CardHolderName.Should().Be("Alice");
        result.Cpf.Should().Be("111.222.333-44");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnZero_WhenNoEntitiesExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        // Act
        var count = await repository.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount_WhenEntitiesExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(CustomerBuilder.Default());
        repository.Add(CustomerBuilder.Default());
        repository.Add(CustomerBuilder.Default());
        await repository.SaveChangesAsync();

        // Act
        var count = await repository.CountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_ShouldApplyPredicate_WhenPredicateIsProvided()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        repository.Add(new CustomerBuilder().WithStatus(false).Build());
        await repository.SaveChangesAsync();

        // Act
        var countActive = await repository.CountAsync(c => c.Status == true);
        var countInactive = await repository.CountAsync(c => c.Status == false);

        // Assert
        countActive.Should().Be(2);
        countInactive.Should().Be(1);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnZero_WhenPredicateMatchesNothing()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        await repository.SaveChangesAsync();

        // Act
        var count = await repository.CountAsync(c => c.Status == false);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnFirstPage_WhenCalledWithPageOne()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        for (int i = 0; i < 15; i++)
            repository.Add(CustomerBuilder.Default());

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetPagedAsync(1, 10);

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnRemainingItems_WhenLastPageIsRequested()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        for (int i = 0; i < 15; i++)
            repository.Add(CustomerBuilder.Default());

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetPagedAsync(2, 10);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyList_WhenNoEntitiesExist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetPagedAsync(1, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplyPredicate_WhenPredicateIsProvided()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        repository.Add(new CustomerBuilder().WithStatus(false).Build());
        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetPagedAsync(1, 10, c => c.Status == true);

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Status).Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyList_WhenPredicateMatchesNothing()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        repository.Add(new CustomerBuilder().WithStatus(true).Build());
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetPagedAsync(1, 10, c => c.Status == false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldSkipCorrectItems_WhenPageTwoIsRequested()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        for (int i = 0; i < 5; i++)
            repository.Add(CustomerBuilder.Default());

        await repository.SaveChangesAsync();

        // Act
        var page1 = await repository.GetPagedAsync(1, 3);
        var page2 = await repository.GetPagedAsync(2, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(2);
        page1.Select(c => c.Id).Should().NotIntersectWith(page2.Select(c => c.Id));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnAffectedRowCount_WhenEntitiesAreSaved()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);
        repository.Add(CustomerBuilder.Default());

        // Act
        var rows = await repository.SaveChangesAsync();

        // Assert
        rows.Should().BeGreaterThan(0);
    }
}
