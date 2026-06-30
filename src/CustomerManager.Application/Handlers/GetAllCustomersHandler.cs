using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using CustomerManager.Domain.Entities;
using CustomerManager.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Handlers
{
    public class GetAllCustomersHandler(ICustomerRepository repository) : IGetAllCustomerHandler
    {
        private readonly ICustomerRepository _repository = repository;

        public async Task<PagedResponse<GetAllCustomerResponse>> Handle(GetAllCustomerQuery query)
        {
            Expression<Func<Customer, bool>> filtro = u => u.Status == !query.Ativos;

            var totalUsers = await _repository.CountAsync(filtro);
            var customers = await _repository.GetPagedAsync(
                query.Pagina,
                query.TamanhoPagina,
                filtro
            );

            var listaUsersResponse = customers.Select(customer => new GetAllCustomerResponse(
                customer.Id,
                customer.CardHolderName,
                customer.Cpf,
                customer.Status
            ));

            var totalPaginas = (int)Math.Ceiling(totalUsers / (double)query.TamanhoPagina);

            return new PagedResponse<GetAllCustomerResponse>(
                listaUsersResponse,
                query.Pagina,
                totalPaginas,
                totalUsers
            );
        }
    }
}
