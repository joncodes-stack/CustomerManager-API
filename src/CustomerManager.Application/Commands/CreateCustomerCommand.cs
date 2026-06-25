namespace CustomerManager.Application.Commands;

public record CreateCustomerCommand(
    Guid Id,
    string CardHolderName,
    string Cpf,
    bool Status
);

