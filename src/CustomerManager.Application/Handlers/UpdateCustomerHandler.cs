using Amazon.SimpleNotificationService.Model;
using CustomerManager.Application.Commands;
using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CustomerManager.Application.Handlers
{
    public class UpdateCustomerHandler(
    ILogger<UpdateCustomerHandler> logger,
    ICustomerRepository repository,
    ICustomerEventPublisher eventPublisher,
    IDistributedCache cache) : IUpdateCustomerHandler
    {
        private readonly ILogger<UpdateCustomerHandler> _logger = logger;
        private readonly ICustomerRepository _repository = repository;
        private readonly ICustomerEventPublisher _eventPublisher = eventPublisher;
        private readonly IDistributedCache _cache = cache;

        public async Task<UpdateCustomerResponse> Handle(UpdateCustomerCommand command)
        {
            var user = await _repository.GetByIdAsync(command.Id);

            if (user == null)
            {
                _logger.LogWarning("Update attempt failed: Customer with ID {Id} not found.", command.Id);
                throw new NotFoundException($"Customer with ID {command.Id} not found.");
            }

            try
            {
                user.Atualizar(command.CardHolderName, command.Cpf);

                _repository.Update(user);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("Customer {Id} updated successfully.", user.Id);

                // invalida o cache pois os dados mudaram
                var hoje = DateTime.UtcNow.ToString("yyyy-MM-dd");
                await _cache.RemoveAsync($"customer:id:{user.Id}:{hoje}");
                await _cache.RemoveAsync($"customer:cpf:{user.Cpf}:{hoje}");

                // publica o evento
                await _eventPublisher.PublicarAsync(new CustomerEventMessage
                {
                    TipoEvento = "ContaAtualizada",
                    CustomerId = user.Id.ToString(),
                    DataEvento = DateTime.UtcNow
                });

                return new UpdateCustomerResponse(
                    user.Id,
                    user.CardHolderName,
                    user.Cpf,
                    user.Status,
                    "Customer updated successfully!"
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while updating customer {Id}.", command.Id);
                throw;
            }
        }
    }
}
