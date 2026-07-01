using CustomerManager.Application.Handlers;
using CustomerManager.Application.Queries;
using CustomerManager.Domain.Entities;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CustomerManager.UnitTests.Application.Handlers;

public class GetAllCustomersHandlerTests
{
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly GetAllCustomersHandler _handler;

    public GetAllCustomersHandlerTests()
    {
        _repositoryMock = MockCreator.CustomerRepository();
        _handler = new GetAllCustomersHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResponse_WhenCustomersExist()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);
        var customers = new List<Customer> { CustomerBuilder.Default(), CustomerBuilder.Default() };

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(2);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.Itens.Should().HaveCount(2);
        result.TotalItens.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(0);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.Itens.Should().BeEmpty();
        result.TotalItens.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPagesCorrectly_WhenTotalExceedsPageSize()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);
        var customers = Enumerable.Range(0, 10).Select(_ => CustomerBuilder.Default()).ToList();

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(25);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.TotalPaginas.Should().Be(3); // Math.Ceiling(25 / 10.0) = 3
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalPagesForExactMultiple_WhenTotalIsMultipleOfPageSize()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);
        var customers = Enumerable.Range(0, 10).Select(_ => CustomerBuilder.Default()).ToList();

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(20);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.TotalPaginas.Should().Be(2); // Math.Ceiling(20 / 10.0) = 2
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCurrentPage_WhenPageTwoIsRequested()
    {
        // Arrange
        var query = new GetAllCustomerQuery(2, 5, true);
        var customers = Enumerable.Range(0, 5).Select(_ => CustomerBuilder.Default()).ToList();

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(10);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.PaginaAtual.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldCallCountAsyncOnce_WhenHandlerIsCalled()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(0);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallGetPagedAsyncOnce_WhenHandlerIsCalled()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(0);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(
            r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapCustomersToResponse_WhenCustomersExist()
    {
        // Arrange
        var query = new GetAllCustomerQuery(1, 10, true);
        var customer = new CustomerBuilder().WithCardHolderName("Alice").WithCpf("111.222.333-44").Build();

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer> { customer });

        // Act
        var result = await _handler.Handle(query);

        // Assert
        var item = result.Itens.Single();
        item.CardHolderName.Should().Be("Alice");
        item.Cpf.Should().Be("111.222.333-44");
        item.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Handle_ShouldPassPageParametersToRepository_WhenQueryHasCustomPagination()
    {
        // Arrange
        var query = new GetAllCustomerQuery(3, 5, false);

        _repositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(0);
        _repositoryMock.Setup(r => r.GetPagedAsync(3, 5, It.IsAny<Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.GetPagedAsync(3, 5, It.IsAny<Expression<Func<Customer, bool>>>()), Times.Once);
    }
}
