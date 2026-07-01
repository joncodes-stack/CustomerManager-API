using Amazon.SimpleNotificationService.Model;
using CustomerManager.Application.Handlers;
using CustomerManager.Domain.Entities;
using CustomerManager.Domain.Interfaces.Services;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.Application.Commands;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CustomerManager.UnitTests.Application.Handlers;

public class UpdateCustomerHandlerTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<UpdateCustomerHandler>> _loggerMock;
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly Mock<ICustomerEventPublisher> _eventPublisherMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly UpdateCustomerHandler _handler;

    public UpdateCustomerHandlerTests()
    {
        _loggerMock = MockCreator.Logger<UpdateCustomerHandler>();
        _repositoryMock = MockCreator.CustomerRepository();
        _eventPublisherMock = MockCreator.EventPublisher();
        _cacheMock = MockCreator.DistributedCache();

        _handler = new UpdateCustomerHandler(
            _loggerMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _cacheMock.Object);
    }

    private void SetupSuccessfulUpdate(Customer customer, UpdateCustomerCommand command)
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder()
            .WithId(customer.Id)
            .WithCardHolderName("Jane Doe")
            .WithCpf("987.654.321-00")
            .Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.CardHolderName.Should().Be("Jane Doe");
        result.Cpf.Should().Be("987.654.321-00");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessMessage_WhenRequestIsValid()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Mensagem.Should().Be("Customer updated successfully!");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = UpdateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce_WhenHandlerIsCalled()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryUpdateOnce_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsyncOnce_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallCacheRemoveTwice_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldPublishContaAtualizadaEvent_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulUpdate(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(
            It.Is<CustomerEventMessage>(m => m.TipoEvento == "ContaAtualizada")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRethrowArgumentException_WhenRepositoryUpdateThrows()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new UpdateCustomerCommandBuilder().WithId(customer.Id).Build();

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.Update(It.IsAny<Customer>()))
            .Throws(new ArgumentException("Invalid argument"));

        // Act
        var act = async () => await _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid argument");
    }

    [Fact]
    public async Task Handle_ShouldNotCallEventPublisher_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = UpdateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<NotFoundException>();

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallCacheRemove_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = UpdateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<NotFoundException>();

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
