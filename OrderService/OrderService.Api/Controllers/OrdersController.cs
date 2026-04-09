using System.Text.Json; // EXISTING: 用来序列化 RabbitMQ 消息
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.DTOs; // NEW: 引入 DTO
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;
    private readonly RabbitMQPublisher _publisher;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient
    )
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;

        // EXISTING: 从环境变量读取 RabbitMQ host
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        // EXISTING: 初始化 publisher
        _publisher = new RabbitMQPublisher(rabbitHost, "orders-queue");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // CHANGED: 不再直接返回 Order entity
        // CHANGED: 改为返回 OrderDto
        var orders = await _context
            .Orders.Select(o => new OrderDto
            {
                Id = o.Id,
                Total = o.Total,
                CustomerId = o.CustomerId,
                ProductId = o.ProductId,
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // NEW: 新增按 id 查询接口，并返回 DTO
        var order = await _context
            .Orders.Where(o => o.Id == id)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                Total = o.Total,
                CustomerId = o.CustomerId,
                ProductId = o.ProductId,
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound("Order not found.");

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        // CHANGED: 不再直接接收 Order entity
        // CHANGED: 改为接收 CreateOrderDto

        var customerExists = await _customerClient.CustomerExistsAsync(dto.CustomerId);
        if (!customerExists)
            return BadRequest("Customer does not exist.");

        var productExists = await _productClient.ProductExistsAsync(dto.ProductId);
        if (!productExists)
            return BadRequest("Product does not exist.");

        // NEW: DTO -> Entity
        var order = new Order
        {
            Total = dto.Total,
            CustomerId = dto.CustomerId,
            ProductId = dto.ProductId,
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // EXISTING: 发布 OrderCreated 事件
        var message = JsonSerializer.Serialize(
            new
            {
                EventType = "OrderCreated",
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Total = order.Total,
            }
        );

        _publisher.Publish(message);

        // CHANGED: 返回 OrderDto，而不是直接返回 Order entity
        var result = new OrderDto
        {
            Id = order.Id,
            Total = order.Total,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
        };

        return Ok(result);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        // EXISTING: 取消订单
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound("Order not found.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // EXISTING: 发布 OrderCancelled 事件
        var cancelMessage = JsonSerializer.Serialize(
            new
            {
                EventType = "OrderCancelled",
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Total = order.Total,
            }
        );

        _publisher.Publish(cancelMessage);

        // CHANGED: 返回 DTO / 简洁结果，不暴露 entity
        var result = new OrderDto
        {
            Id = order.Id,
            Total = order.Total,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
        };

        return Ok(new { Message = $"Order {id} cancelled and event published.", Order = result });
    }
}
