using CustomerManager.Application.Commands;
using CustomerManager.Application.Responses;

namespace CustomerManager.Application.Interfaces
{
    public interface IDeleteCustomerHandler
    {
        Task<DeleteCustomerResponse> Handle(DeleteCustomerCommand command);
    }
}
