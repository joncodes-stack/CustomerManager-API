using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomerManager.Application.Handlers
{
    public class GetCustomerByCpfHandler(
    ICustomerRepository repository,
    IDistributedCache cache) : IGetCustomerByCPFHandler
    {
        private readonly ICustomerRepository _repository = repository;
        private readonly IDistributedCache _cache = cache;

        public async Task<GetCustomerByCpfResponse?> Handle(GetCustomerByCpfQuery query)
        {
            var chave = $"customer:cpf:{query.Cpf}:{DateTime.UtcNow:yyyy-MM-dd}";

            var cached = await _cache.GetStringAsync(chave);
            if (cached is not null)
                return JsonSerializer.Deserialize<GetCustomerByCpfResponse>(cached);

            var customer = await _repository.GetCustomerByCpfAsync(query.Cpf);

            if (customer is null)
                return null;

            var response = new GetCustomerByCpfResponse(
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
