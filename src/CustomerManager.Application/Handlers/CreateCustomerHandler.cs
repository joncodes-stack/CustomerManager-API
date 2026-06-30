using CustomerManager.Application.Commands;
using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Entities;
using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Domain.Interfaces.Services;
using CustomerManager.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace CustomerManager.Application.Handlers
{
    public class CreateCustomerHandler(
    ILogger<CreateCustomerHandler> logger,
    ICustomerRepository repository,
    ICustomerEventPublisher eventPublisher) : ICreateCustomerHandler
    {
        private readonly ILogger<CreateCustomerHandler> _logger = logger;
        private readonly ICustomerRepository _repository = repository;
        private readonly ICustomerEventPublisher _eventPublisher = eventPublisher;

        public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand command)
        {
            var alreadyExists = await _repository.ExistsByCpfAsync(command.Cpf);
            if (alreadyExists)
            {
                _logger.LogWarning("Já existe um usuário com o CPF: {Cpf}", command.Cpf);
                throw new AlreadyExistsException("Já existe um usuário com esse CPF");
            }

            var user = new Customer(
                command.CardHolderName,
                command.Cpf,
                true
            );

            _repository.Add(user);
            await _repository.SaveChangesAsync();

            // publica o evento após persistir com sucesso
            await _eventPublisher.PublicarAsync(new CustomerEventMessage
            {
                TipoEvento = "ContaCriada",
                CustomerId = user.Id.ToString(),
                DataEvento = DateTime.UtcNow
            });

            return new CreateCustomerResponse(
                user.Id,
                user.CardHolderName,
                user.Cpf,
                user.Status
            );
        }
    }
}
