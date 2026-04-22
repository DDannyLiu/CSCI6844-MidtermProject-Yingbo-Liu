using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("aggregate/customerdetails")]
public class CustomerDetailsController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public CustomerDetailsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var response = await _httpClient.GetAsync("http://customerservice:8080/api/customers");
        var content = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, content);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var response = await _httpClient.GetAsync(
            $"http://customerservice:8080/api/customers/{id}"
        );

        var content = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://customerservice:8080/api/customers",
            dto
        );

        var content = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, content);
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
