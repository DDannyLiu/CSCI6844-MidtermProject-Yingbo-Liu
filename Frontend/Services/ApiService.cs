using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductDto>?> GetProductsAsync()
        {
            return await _http.GetFromJsonAsync<List<ProductDto>>("/aggregate/productdetails");
        }

        public async Task<HttpResponseMessage> CreateProductAsync(ProductDto dto)
        {
            return await _http.PostAsJsonAsync("/aggregate/productdetails", dto);
        }

        public async Task<List<CustomerDto>?> GetCustomersAsync()
        {
            return await _http.GetFromJsonAsync<List<CustomerDto>>("/aggregate/customerdetails");
        }

        public async Task<HttpResponseMessage> CreateCustomerAsync(CustomerDto dto)
        {
            return await _http.PostAsJsonAsync("/aggregate/customerdetails", dto);
        }

        public async Task<List<OrderDto>?> GetOrdersAsync()
        {
            return await _http.GetFromJsonAsync<List<OrderDto>>("/aggregate/orderdetails");
        }

        public async Task<HttpResponseMessage> CreateOrderAsync(CreateOrderDto dto)
        {
            return await _http.PostAsJsonAsync("/aggregate/orderdetails", dto);
        }

        public async Task<HttpResponseMessage> CancelOrderAsync(int id)
        {
            return await _http.PostAsync($"/aggregate/orderdetails/cancel/{id}", null);
        }
    }
}
