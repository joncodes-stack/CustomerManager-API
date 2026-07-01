using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CustomerManager.Application.Handlers
{
    public class GetCustomerByIdHandler(
    ICustomerRepository repository,
    IDistributedCache cache) : IGetCustomerByIdHandler
    {
        private readonly ICustomerRepository _repository = repository;
        private readonly IDistributedCache _cache = cache;

        public async Task<GetCustomerByIdResponse?> Handle(GetCustomerByIdQuery query)
        {
            var chave = $"customer:id:{query.Id}:{DateTime.UtcNow:yyyy-MM-dd}";

            var cached = await _cache.GetStringAsync(chave);
            if (cached is not null)
                return JsonSerializer.Deserialize<GetCustomerByIdResponse>(cached);

            var customer = await _repository.GetByIdAsync(query.Id);

            if (customer is null || !customer.Status)
                return null;

            var response = new GetCustomerByIdResponse(
                customer.Id,
                customer.CardHolderName,
                customer.Cpf,
                customer.Status
            );

            var expiracao = DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow;

            await _cache.SetStringAsync(chave, JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiracao
                });

            return response;
        }
    }
}
