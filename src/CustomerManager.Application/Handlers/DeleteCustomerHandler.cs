using Amazon.SimpleNotificationService.Model;
using CustomerManager.Application.Commands;
using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Infra.Messaging;
using CustomerManager.Infra.Messaging.Event;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Handlers
{
    public class DeleteCustomerHandler(
    ILogger<DeleteCustomerHandler> logger,
    ICustomerRepository repository,
    ICustomerEventPublisher eventPublisher) : IDeleteCustomerHandler
    {
        private readonly ILogger<DeleteCustomerHandler> _logger = logger;
        private readonly ICustomerRepository _repository = repository;
        private readonly ICustomerEventPublisher _eventPublisher = eventPublisher;

        public async Task<DeleteCustomerResponse> Handle(DeleteCustomerCommand command)
        {
            var customer = await _repository.GetByIdAsync(command.Id);

            if (customer == null)
            {
                _logger.LogWarning("Tentativa de inativar usuário inexistente: {Id}", command.Id);
                throw new NotFoundException("Usuário não encontrado.");
            }

            customer.Inativar();

            _repository.Update(customer);
            await _repository.SaveChangesAsync();

            // publica o evento após persistir com sucesso
            await _eventPublisher.PublicarAsync(new CustomerEvent
            {
                TipoEvento = "ContaDeletada",
                CustomerId = customer.Id.ToString(),
                OcorridoEm = DateTime.UtcNow
            });

            return new DeleteCustomerResponse(
                customer.Id,
                customer.CardHolderName,
                customer.Cpf,
                customer.Status,
                $"O usuário {customer.CardHolderName} foi inativado com sucesso."
            );
        }
    }
}
