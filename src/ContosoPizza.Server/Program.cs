using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ContosoPizza.Services.PizzaService>();
builder.Services.AddScoped<ContosoPizza.Repositories.UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ContosoPizza.Services.UserService>();

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
