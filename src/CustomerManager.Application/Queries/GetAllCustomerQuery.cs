namespace CustomerManager.Application.Queries;

public record GetAllCustomerQuery(
    int Pagina = 1,
    int TamanhoPagina = 10,
    bool Ativos = true
);

