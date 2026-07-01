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

public class DeleteCustomerHandlerTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<DeleteCustomerHandler>> _loggerMock;
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly Mock<ICustomerEventPublisher> _eventPublisherMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly DeleteCustomerHandler _handler;

    public DeleteCustomerHandlerTests()
    {
        _loggerMock = MockCreator.Logger<DeleteCustomerHandler>();
        _repositoryMock = MockCreator.CustomerRepository();
        _eventPublisherMock = MockCreator.EventPublisher();
        _cacheMock = MockCreator.DistributedCache();

        _handler = new DeleteCustomerHandler(
            _loggerMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _cacheMock.Object);
    }

    private void SetupSuccessfulDelete(Customer customer, DeleteCustomerCommand command)
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ShouldInactivateCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessMessage_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Mensagem.Should().Be($"O usuário {customer.CardHolderName} foi inativado com sucesso.");
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerData_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Id.Should().Be(customer.Id);
        result.CardHolderName.Should().Be(customer.CardHolderName);
        result.Cpf.Should().Be(customer.Cpf);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = DeleteCustomerCommandBuilder.Default();
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
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

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
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

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
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

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
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldPublishContaDeletadaEvent_WhenCustomerExists()
    {
        // Arrange
        var customer = CustomerBuilder.Default();
        var command = new DeleteCustomerCommandBuilder().WithId(customer.Id).Build();
        SetupSuccessfulDelete(customer, command);

        // Act
        await _handler.Handle(command);

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(
            It.Is<CustomerEventMessage>(m => m.TipoEvento == "ContaDeletada")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallRepositoryUpdate_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = DeleteCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<NotFoundException>();

        // Assert
        _repositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallEventPublisher_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = DeleteCustomerCommandBuilder.Default();
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
        var command = DeleteCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<NotFoundException>();

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
