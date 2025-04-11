
using Microsoft.AspNetCore.Mvc;
using SidoAgung.ApiSaga.Infrastruktur.Models;
using SidoAgung.ApiSaga.Infrastruktur.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Controllers;
    [Route("wongnormal/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [HttpGet]
         //[ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "*" })]
        public async Task<IEnumerable<CustomerModel>> GetCustomers() =>
            await _customerRepository.GetCustomers();

        [HttpGet("{id}")]
         //[ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "id" })]
        public async Task<ActionResult<CustomerModel>> GetCustomer(int id)
        {
            var customer = await _customerRepository.GetCustomerById(id);
            if (customer == null) return NotFound();
            return customer;
        }

        [HttpPost]
        public async Task<ActionResult> PostCustomer(CustomerModel customer)
        {
            await _customerRepository.AddCustomer(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerModel customer)
        {
            if (id != customer.Id) return BadRequest();
            await _customerRepository.UpdateCustomer(customer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await _customerRepository.DeleteCustomer(id);
            return NoContent();
        }
    }

