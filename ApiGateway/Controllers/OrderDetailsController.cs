using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("aggregate/orderdetails")]
public class OrderDetailsController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public OrderDetailsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var response = await _httpClient.GetAsync("http://orderservice:8080/api/orders");

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(int id)
    {
        var orderResponse = await _httpClient.GetAsync("http://orderservice:8080/api/orders");

        if (!orderResponse.IsSuccessStatusCode)
            return BadRequest("Could not get order data.");

        var orders = await orderResponse.Content.ReadFromJsonAsync<List<OrderDto>>();

        if (orders == null)
            return BadRequest("Order list is empty.");

        var order = orders.FirstOrDefault(o => o.Id == id);

        if (order == null)
            return NotFound("Order not found.");

        var customerResponse = await _httpClient.GetAsync(
            $"http://customerservice:8080/api/customers/{order.CustomerId}"
        );

        var customerJson = await customerResponse.Content.ReadAsStringAsync();

        var productResponse = await _httpClient.GetAsync(
            $"http://productservice:8080/api/products/{order.ProductId}"
        );

        var productJson = await productResponse.Content.ReadAsStringAsync();

        return Content(
            $@"{{
    ""order"": {JsonSerializer.Serialize(order)},
    ""customer"": {customerJson},
    ""product"": {productJson}
}}",
            "application/json"
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://orderservice:8080/api/orders",
            dto
        );

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var response = await _httpClient.PostAsync(
            $"http://orderservice:8080/api/orders/cancel/{id}",
            null
        );

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    private class OrderDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }

    public class CreateOrderDto
    {
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }
}
