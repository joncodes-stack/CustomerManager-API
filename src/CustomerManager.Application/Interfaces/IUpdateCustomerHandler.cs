using CustomerManager.Application.Commands;
using CustomerManager.Application.Responses;

namespace CustomerManager.Application.Interfaces
{
    public interface IUpdateCustomerHandler
    {
        Task<UpdateCustomerResponse> Handle(UpdateCustomerCommand command);
    }
}
