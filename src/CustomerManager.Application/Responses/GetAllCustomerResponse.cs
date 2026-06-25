using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Responses;

public record GetAllCustomerResponse(Guid Id,
    string CardHolderName,
    string Cpf,
    bool Status);
