using CustomerManager.Domain.Entities;

namespace CustomerManager.UnitTests.Shared.Builders;

public class CustomerBuilder
{
    private string _cardHolderName = "John Doe";
    private string _cpf = "123.456.789-09";
    private bool _status = true;

    public CustomerBuilder WithCardHolderName(string name)
    {
        _cardHolderName = name;
        return this;
    }

    public CustomerBuilder WithCpf(string cpf)
    {
        _cpf = cpf;
        return this;
    }

    public CustomerBuilder WithStatus(bool status)
    {
        _status = status;
        return this;
    }

    public Customer Build() => new Customer(_cardHolderName, _cpf, _status);

    public static Customer Default() => new CustomerBuilder().Build();

    public static Customer WithInactiveStatus() => new CustomerBuilder().WithStatus(false).Build();
}
