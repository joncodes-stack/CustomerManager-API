using CustomerManager.Domain.Entities;
using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Infra.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Infra.Repositories
{
    public class CustomerRepository(CustomerContext context) : BaseRepository<Customer>(context), ICustomerRepository
    {
        public async Task<bool> ExistsByCpfAsync(string cpf)
        => await _context.Set<Customer>()
            .AnyAsync(u => u.Cpf == cpf && u.Status == true);

        public async Task<Customer?> GetCustomerByCpfAsync(string cpf)
        => await _context.Set<Customer>()
            .FirstOrDefaultAsync(u => u.Cpf == cpf && u.Status == true);
    }
}
