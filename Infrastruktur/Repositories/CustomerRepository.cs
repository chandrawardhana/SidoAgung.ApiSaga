

using Microsoft.EntityFrameworkCore;
using SidoAgung.ApiSaga.Infrastruktur.Persistences;
using SidoAgung.ApiSaga.Infrastruktur.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Repositories;
public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerModel>> GetCustomers() =>
            await _context.Customers.ToListAsync();

        public async Task<CustomerModel> GetCustomerById(int id) =>
            await _context.Customers.FindAsync(id);

        public async Task AddCustomer(CustomerModel customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomer(CustomerModel customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
