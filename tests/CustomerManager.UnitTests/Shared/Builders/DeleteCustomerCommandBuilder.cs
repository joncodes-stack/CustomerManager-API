using CustomerManager.Application.Commands;

namespace CustomerManager.UnitTests.Shared.Builders;

public class DeleteCustomerCommandBuilder
{
    private Guid _id = Guid.NewGuid();

    public DeleteCustomerCommandBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DeleteCustomerCommand Build() => new DeleteCustomerCommand(_id);

    public static DeleteCustomerCommand Default() =>
        new DeleteCustomerCommandBuilder().Build();
}
