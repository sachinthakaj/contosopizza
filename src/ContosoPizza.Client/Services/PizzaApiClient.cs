using System.Net.Http.Json;
using ContosoPizza.Models;

namespace ContosoPizza.Client.Services;

public sealed class PizzaApiClient
{
    private readonly HttpClient _http;

    public PizzaApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Pizza>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pizzas = await _http.GetFromJsonAsync<List<Pizza>>("api/pizza", cancellationToken);
        return pizzas ?? [];
    }

    public Task<Pizza?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<Pizza>($"api/pizza/{id}", cancellationToken);

    public async Task<Pizza> AddAsync(Pizza pizza, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/pizza", pizza, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Pizza>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(Pizza pizza, CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsJsonAsync($"api/pizza/{pizza.Id}", pizza, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync($"api/pizza/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
