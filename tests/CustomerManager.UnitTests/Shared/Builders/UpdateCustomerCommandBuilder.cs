using CustomerManager.Application.Commands;

namespace CustomerManager.UnitTests.Shared.Builders;

public class UpdateCustomerCommandBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _cardHolderName = "Jane Doe";
    private string _cpf = "987.654.321-00";

    public UpdateCustomerCommandBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UpdateCustomerCommandBuilder WithCardHolderName(string name)
    {
        _cardHolderName = name;
        return this;
    }

    public UpdateCustomerCommandBuilder WithCpf(string cpf)
    {
        _cpf = cpf;
        return this;
    }

    public UpdateCustomerCommand Build() =>
        new UpdateCustomerCommand(_id, _cardHolderName, _cpf);

    public static UpdateCustomerCommand Default() =>
        new UpdateCustomerCommandBuilder().Build();
}
