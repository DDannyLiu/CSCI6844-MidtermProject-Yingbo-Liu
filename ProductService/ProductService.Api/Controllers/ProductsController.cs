using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.DTOs; // NEW: 引入 DTO
using ProductService.Api.Models;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;

    public ProductsController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id) // CHANGED: 返回 ProductDto，不再直接返回 Product entity
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        // CHANGED: Entity -> DTO
        var result = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto) // CHANGED: 不再直接接收 Product entity，改为接收 CreateProductDto
    {
        // NEW: DTO -> Entity
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // NEW: 返回 DTO，而不是直接返回 entity
        var result = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
        };

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, result);
    }
}
