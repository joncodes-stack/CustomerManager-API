using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Commands;

public record UpdateCustomerCommand(
    Guid Id,
    string CardHolderName,
    string Cpf
);

