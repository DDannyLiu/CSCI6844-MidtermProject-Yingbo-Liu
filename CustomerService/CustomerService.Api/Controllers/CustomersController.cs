using CustomerService.Api.Data;
using CustomerService.Api.DTOs;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _context
            .Customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        var result = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var customer = new Customer { Name = dto.Name, Email = dto.Email };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, result);
    }
}
