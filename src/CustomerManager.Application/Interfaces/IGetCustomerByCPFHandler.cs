
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;

namespace CustomerManager.Application.Interfaces
{
    public interface IGetCustomerByCPFHandler
    {
        Task<GetCustomerByCpfResponse?> Handle(GetCustomerByCpfQuery query);
    }
}
