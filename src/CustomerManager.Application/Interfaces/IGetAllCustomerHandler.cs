using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;

namespace CustomerManager.Application.Interfaces
{
    public interface IGetAllCustomerHandler
    {
        Task<PagedResponse<GetAllCustomerResponse>> Handle(GetAllCustomerQuery query);
    }
}
