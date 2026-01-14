using ContosoPizza.Models;
using ContosoPizza.Data;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Services;

public class PizzaService
{
    private readonly AppDbContext _db;

    public PizzaService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Pizza>> GetAllAsync() =>
        _db.Pizzas.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

    public Task<Pizza?> GetAsync(int id) =>
        _db.Pizzas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Pizza pizza)
    {
        _db.Pizzas.Add(pizza);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pizza = await _db.Pizzas.FirstOrDefaultAsync(p => p.Id == id);
        if (pizza is null)
            return false;

        _db.Pizzas.Remove(pizza);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Pizza pizza)
    {
        var existing = await _db.Pizzas.FirstOrDefaultAsync(p => p.Id == pizza.Id);
        if (existing is null)
            return false;

        existing.Name = pizza.Name;
        existing.IsGlutenFree = pizza.IsGlutenFree;
        await _db.SaveChangesAsync();
        return true;
    }
}