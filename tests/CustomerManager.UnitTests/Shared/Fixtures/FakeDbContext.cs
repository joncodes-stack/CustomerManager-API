using CustomerManager.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace CustomerManager.UnitTests.Shared.Fixtures;

public static class FakeDbContext
{
    public static CustomerContext Create()
    {
        var options = new DbContextOptionsBuilder<CustomerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerContext(options);
    }
}
