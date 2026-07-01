using CustomerManager.Application.Handlers;
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Entities;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;

namespace CustomerManager.UnitTests.Application.Handlers;

public class GetCustomerByCpfHandlerTests
{
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly GetCustomerByCpfHandler _handler;

    public GetCustomerByCpfHandlerTests()
    {
        _repositoryMock = MockCreator.CustomerRepository();
        _cacheMock = MockCreator.DistributedCache();

        _handler = new GetCustomerByCpfHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    private void SetupCacheMiss()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
    }

    private void SetupCacheHit(GetCustomerByCpfResponse response)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);
    }

    private void SetupCacheSet()
    {
        _cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByCpfQuery(customer.Cpf);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Cpf.Should().Be(customer.Cpf);
        result.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var query = new GetCustomerByCpfQuery("000.000.000-00");

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedData_WhenCacheHasData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cachedResponse = new GetCustomerByCpfResponse(id, "Cached User", "123.456.789-09", true);
        var query = new GetCustomerByCpfQuery("123.456.789-09");

        SetupCacheHit(cachedResponse);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task Handle_ShouldNotCallRepository_WhenCacheHasData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cachedResponse = new GetCustomerByCpfResponse(id, "Cached User", "123.456.789-09", true);
        var query = new GetCustomerByCpfQuery("123.456.789-09");

        SetupCacheHit(cachedResponse);

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.GetCustomerByCpfAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGetCustomerByCpfAsyncOnce_WhenCacheIsEmpty()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByCpfQuery(customer.Cpf);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.GetCustomerByCpfAsync(query.Cpf), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveResponseToCache_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByCpfQuery(customer.Cpf);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        await _handler.Handle(query);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSaveToCache_WhenCustomerDoesNotExist()
    {
        // Arrange
        var query = new GetCustomerByCpfQuery("000.000.000-00");

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync((Customer?)null);

        // Act
        await _handler.Handle(query);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMapCustomerToResponse_WhenCustomerExists()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithCardHolderName("Maria Silva")
            .WithCpf("444.555.666-77")
            .Build();
        var query = new GetCustomerByCpfQuery(customer.Cpf);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetCustomerByCpfAsync(query.Cpf)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result!.CardHoldName.Should().Be("Maria Silva");
        result.Status.Should().BeTrue();
    }
}
