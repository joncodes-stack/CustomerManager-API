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

public class GetCustomerByIdHandlerTests
{
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly GetCustomerByIdHandler _handler;

    public GetCustomerByIdHandlerTests()
    {
        _repositoryMock = MockCreator.CustomerRepository();
        _cacheMock = MockCreator.DistributedCache();

        _handler = new GetCustomerByIdHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    private void SetupCacheMiss()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
    }

    private void SetupCacheHit(GetCustomerByIdResponse response)
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
    public async Task Handle_ShouldReturnCustomer_WhenActiveCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByIdQuery(customer.Id);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.Cpf.Should().Be(customer.Cpf);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerIsInactive()
    {
        // Arrange
        var customer = CustomerBuilder.WithInactiveStatus();
        var query = new GetCustomerByIdQuery(customer.Id);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync(customer);

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
        var cachedResponse = new GetCustomerByIdResponse(id, "Cached User", "999.888.777-66", true);
        var query = new GetCustomerByIdQuery(id);

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
        var cachedResponse = new GetCustomerByIdResponse(id, "Cached User", "999.888.777-66", true);
        var query = new GetCustomerByIdQuery(id);

        SetupCacheHit(cachedResponse);

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce_WhenCacheIsEmpty()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByIdQuery(customer.Id);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync(customer);
        SetupCacheSet();

        // Act
        await _handler.Handle(query);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(query.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveResponseToCache_WhenCustomerExistsAndIsActive()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var query = new GetCustomerByIdQuery(customer.Id);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync(customer);
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
    public async Task Handle_ShouldNotSaveToCache_WhenCustomerIsNull()
    {
        // Arrange
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync((Customer?)null);

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
    public async Task Handle_ShouldNotSaveToCache_WhenCustomerIsInactive()
    {
        // Arrange
        var customer = CustomerBuilder.WithInactiveStatus();
        var query = new GetCustomerByIdQuery(customer.Id);

        SetupCacheMiss();
        _repositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync(customer);

        // Act
        await _handler.Handle(query);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
