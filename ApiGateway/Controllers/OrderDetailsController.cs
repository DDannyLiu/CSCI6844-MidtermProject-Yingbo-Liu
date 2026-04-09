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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(int id)
    {
        Console.WriteLine("=== NEW CONTROLLER VERSION ===");

        var orderResponse = await _httpClient.GetAsync("http://orderservice:8080/api/orders");

        if (!orderResponse.IsSuccessStatusCode)
            return BadRequest("Could not get order data.");

        var orders = await orderResponse.Content.ReadFromJsonAsync<List<OrderDto>>();

        if (orders == null)
            return BadRequest("Order list is empty.");

        var order = orders.FirstOrDefault(o => o.Id == id);

        if (order == null)
            return NotFound("Order not found.");

        // customer
        var customerResponse = await _httpClient.GetAsync(
            $"http://customerservice:8080/api/customers/{order.CustomerId}"
        );

        var customerJson = await customerResponse.Content.ReadAsStringAsync();
        var customer = JsonDocument.Parse(customerJson).RootElement.Clone(); // ⭐ FIX

        // product
        var productResponse = await _httpClient.GetAsync(
            $"http://productservice:8080/api/products/{order.ProductId}"
        );

        var productJson = await productResponse.Content.ReadAsStringAsync();
        var product = JsonDocument.Parse(productJson).RootElement.Clone(); // ⭐ FIX

        return Content(
            $@"{{
        ""order"": {JsonSerializer.Serialize(order)},
        ""customer"": {customerJson},
        ""product"": {productJson}
    }}",
            "application/json"
        );
    }

    private class OrderDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }
}
