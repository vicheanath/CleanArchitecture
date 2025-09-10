# Product and Order API Demo

This API demonstrates a Clean Architecture implementation with Products and Orders using CQRS pattern from the Shared project (not MediatR).

## Base URL

```
http://localhost:5105
```

## API Endpoints

### Products

#### 1. Create Product

```http
POST /api/products
Content-Type: application/json

{
    "name": "Laptop",
    "description": "High-performance gaming laptop",
    "price": 1299.99
}
```

#### 2. Get All Products

```http
GET /api/products
```

#### 3. Get Product by ID

```http
GET /api/products/{id}
```

### Orders

#### 1. Create Order

```http
POST /api/orders
Content-Type: application/json

{
    "customerName": "John Doe",
    "customerEmail": "john.doe@example.com",
    "items": [
        {
            "productId": "00000000-0000-0000-0000-000000000000",
            "quantity": 2
        }
    ]
}
```

#### 2. Get All Orders

```http
GET /api/orders
```

#### 3. Get Order by ID

```http
GET /api/orders/{id}
```

## Testing Steps

1. **Create a Product**: Use the "Create Product" endpoint to add a product
2. **Get Products**: Verify the product was created using "Get All Products"
3. **Create an Order**: Use the product ID from step 1 to create an order
4. **Get Orders**: Verify the order was created using "Get All Orders"

## Architecture Features

- **Clean Architecture**: Proper separation of concerns across layers
- **CQRS Pattern**: Using commands and queries from the Shared project
- **In-Memory Database**: File-based storage using ConcurrentDictionary
- **Domain-Driven Design**: Rich domain models with business logic
- **Result Pattern**: Proper error handling with Result<T> pattern
- **No MediatR Dependency**: Using custom CQRS implementation

## Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Custom CQRS**: No external dependencies like MediatR
- **In-Memory Storage**: Simple file-based persistence
- **Clean Architecture**: Proper layered architecture
