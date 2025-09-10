using Clean.Architecture.Persistence;
using Shared.Extensions;
using Clean.Architecture.Api.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CQRS - register handlers from Application assembly
builder.Services.AddCqrs(typeof(Clean.Architecture.Application.Products.CreateProduct.CreateProductCommand).Assembly);

// Add Persistence layer
builder.Services.AddPersistence();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
