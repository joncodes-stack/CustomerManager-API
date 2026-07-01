using CustomerManager.Application.Handlers;
using CustomerManager.Domain.Entities;
using CustomerManager.Domain.Interfaces.Services;
using CustomerManager.Shared.Exceptions;
using CustomerManager.UnitTests.Shared.Builders;
using CustomerManager.UnitTests.Shared.Fixtures;
using FluentAssertions;
using Moq;

namespace CustomerManager.UnitTests.Application.Handlers;

public class CreateCustomerHandlerTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<CreateCustomerHandler>> _loggerMock;
    private readonly Mock<CustomerManager.Domain.Interfaces.Repositories.ICustomerRepository> _repositoryMock;
    private readonly Mock<ICustomerEventPublisher> _eventPublisherMock;
    private readonly CreateCustomerHandler _handler;

    public CreateCustomerHandlerTests()
    {
        _loggerMock = MockCreator.Logger<CreateCustomerHandler>();
        _repositoryMock = MockCreator.CustomerRepository();
        _eventPublisherMock = MockCreator.EventPublisher();

        _handler = new CreateCustomerHandler(
            _loggerMock.Object,
            _repositoryMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.CardHolderName.Should().Be(command.CardHolderName);
        result.Cpf.Should().Be(command.Cpf);
        result.Status.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnResponseWithNonEmptyId_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowAlreadyExistsException_WhenCpfAlreadyExists()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("Já existe um usuário com esse CPF");
    }

    [Fact]
    public async Task Handle_ShouldCallExistsByCpfAsyncOnce_WhenHandlerIsCalled()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.ExistsByCpfAsync(command.Cpf), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsyncOnce_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishContaCriadaEvent_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(
            It.Is<CustomerEventMessage>(m => m.TipoEvento == "ContaCriada")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallRepositoryAdd_WhenCpfAlreadyExists()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<AlreadyExistsException>();

        // Assert
        _repositoryMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallEventPublisher_WhenCpfAlreadyExists()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<AlreadyExistsException>();

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenCpfAlreadyExists()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();
        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command);
        await act.Should().ThrowAsync<AlreadyExistsException>();

        // Assert
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectCustomerId_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateCustomerCommandBuilder.Default();

        _repositoryMock.Setup(r => r.ExistsByCpfAsync(command.Cpf)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _eventPublisherMock.Setup(e => e.PublicarAsync(It.IsAny<CustomerEventMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _eventPublisherMock.Verify(e => e.PublicarAsync(
            It.Is<CustomerEventMessage>(m => m.CustomerId == result.Id.ToString())), Times.Once);
    }
}
