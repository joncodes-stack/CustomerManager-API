using CustomerManager.Domain.Entities;

namespace CustomerManager.Domain.Interfaces.Repositories
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Task<bool> ExistsByCpfAsync(string cpf);
        Task<Customer?> GetCustomerByCpfAsync(string cpf);
    }
}
