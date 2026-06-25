using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Interfaces
{
    public interface IGetCustomerByIdHandler
    {
        Task<GetCustomerByIdResponse?> Handle(GetCustomerByIdQuery query);
    }
}
