using CustomerManager.Application.Commands;

namespace CustomerManager.UnitTests.Shared.Builders;

public class CreateCustomerCommandBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _cardHolderName = "John Doe";
    private string _cpf = "123.456.789-09";
    private bool _status = true;

    public CreateCustomerCommandBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CreateCustomerCommandBuilder WithCardHolderName(string name)
    {
        _cardHolderName = name;
        return this;
    }

    public CreateCustomerCommandBuilder WithCpf(string cpf)
    {
        _cpf = cpf;
        return this;
    }

    public CreateCustomerCommand Build() =>
        new CreateCustomerCommand(_id, _cardHolderName, _cpf, _status);

    public static CreateCustomerCommand Default() =>
        new CreateCustomerCommandBuilder().Build();
}
