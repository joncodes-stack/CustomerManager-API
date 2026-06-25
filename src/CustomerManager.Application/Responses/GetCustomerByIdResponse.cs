using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Responses;

public record GetCustomerByIdResponse(
    Guid Id,
    string CardHoldName,
    string Cpf,
    bool Status
);
