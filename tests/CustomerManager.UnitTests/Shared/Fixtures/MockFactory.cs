using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerManager.UnitTests.Shared.Fixtures;

public static class MockCreator
{
    public static Mock<ICustomerRepository> CustomerRepository() => new();
    public static Mock<ICustomerEventPublisher> EventPublisher() => new();
    public static Mock<IDistributedCache> DistributedCache() => new();
    public static Mock<ILogger<T>> Logger<T>() => new();
}
