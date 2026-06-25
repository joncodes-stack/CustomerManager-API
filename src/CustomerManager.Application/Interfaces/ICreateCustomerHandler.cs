using CustomerManager.Application.Commands;
using CustomerManager.Application.Responses;

namespace CustomerManager.Application.Interfaces
{
    public interface ICreateCustomerHandler
    {
        Task<CreateCustomerResponse> Handle(CreateCustomerCommand command);
    }
}
