using ContosoPizza.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ContosoPizza.Services.PizzaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Pizzas.Any())
    {
        db.Pizzas.AddRange(
            new ContosoPizza.Models.Pizza { Name = "Classic Italian", IsGlutenFree = false },
            new ContosoPizza.Models.Pizza { Name = "Veggie", IsGlutenFree = true });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();

app.UseCors("ClientCors");

app.MapControllers();

app.Run();
