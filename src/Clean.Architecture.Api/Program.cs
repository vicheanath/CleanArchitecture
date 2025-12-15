using Clean.Architecture.Persistence;
using Clean.Architecture.Api.Middleware;
using Clean.Architecture.Api.Filters;
using Clean.Architecture.Api.Extensions;
using Clean.Architecture.Application.Auth;
using Clean.Architecture.Application.Common.Authorization;
using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Application.Common.Services;
using Clean.Architecture.Domain.Users;
using Microsoft.AspNetCore.Authorization;
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
builder.Services.AddAuth();

// Add Persistence layer
builder.Services.AddPersistence();

// Add HTTP Context Accessor for permission checking
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization with permission policies - automatically register all permissions
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permission.All)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

// Register permission handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

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

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
