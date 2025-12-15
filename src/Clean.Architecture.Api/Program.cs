using Clean.Architecture.Persistence;
using Clean.Architecture.Api.Middleware;
using Clean.Architecture.Api.Filters;
using Scalar.AspNetCore;
using Shared.Messaging;
using Clean.Architecture.Application.Products;
using Clean.Architecture.Application.Orders;
using Clean.Architecture.Application.Inventory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultActionFilter>();
});
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register Domain Event Publisher
builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

// Register Features
builder.Services.AddProducts();
builder.Services.AddOrders();
builder.Services.AddInventory();

// Add Persistence layer
builder.Services.AddPersistence();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Seed initial data in development
    if (app.Environment.IsDevelopment())
    {
        await DatabaseSeeder.SeedAsync(context);
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
