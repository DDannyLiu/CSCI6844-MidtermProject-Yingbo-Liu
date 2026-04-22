using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("aggregate/productdetails")]
public class ProductDetailsController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ProductDetailsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var response = await _httpClient.GetAsync("http://productservice:8080/api/products");

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var response = await _httpClient.GetAsync($"http://productservice:8080/api/products/{id}");

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://productservice:8080/api/products",
            dto
        );

        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, content);
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
