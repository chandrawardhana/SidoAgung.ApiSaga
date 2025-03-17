using SidoAgung.ApiSaga.Infrastruktur.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Repositories;
public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerModel>> GetCustomers();
        Task<CustomerModel> GetCustomerById(int id);
        Task AddCustomer(CustomerModel customer);
        Task UpdateCustomer(CustomerModel customer);
        Task DeleteCustomer(int id);
    } 
