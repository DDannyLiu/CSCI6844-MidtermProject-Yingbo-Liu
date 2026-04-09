using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using PaymentService.Api.DTOs; // NEW: 引入 DTO
using PaymentService.Api.Models;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _context;

    public PaymentController(PaymentDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id) // CHANGED: 返回 PaymentDto，不再直接返回 entity
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
            return NotFound();

        // NEW: Entity -> DTO
        var result = new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentDto dto) // CHANGED: 不再接收 Payment entity
    {
        // NEW: DTO -> Entity
        var payment = new Payment { OrderId = dto.OrderId, Amount = dto.Amount };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        // NEW: 返回 DTO
        var result = new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
        };

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, result);
    }
}
