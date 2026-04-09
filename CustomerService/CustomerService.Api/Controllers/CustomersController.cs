using CustomerService.Api.Data;
using CustomerService.Api.DTOs; // NEW: 引入 DTO
using CustomerService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _context;

    public CustomersController(CustomerDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id) // CHANGED: 返回 CustomerDto，不再直接返回 Customer entity
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        // CHANGED: Entity -> DTO
        var result = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto) // CHANGED: 不再直接接收 Customer entity，改为接收 CreateCustomerDto
    {
        // NEW: DTO -> Entity
        var customer = new Customer { Name = dto.Name, Email = dto.Email };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // NEW: 返回 DTO，而不是直接返回 entity
        var result = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, result);
    }
}
