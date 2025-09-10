## Binary File Storage Test

This document demonstrates how to test the binary file storage implementation.

### Test the API

1. **Start the API** (already running on port 5105)

2. **Create a Product**:

```bash
curl -X POST "http://localhost:5105/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "LAPTOP-001",
    "name": "Gaming Laptop",
    "description": "High-performance gaming laptop with RTX graphics",
    "price": 1299.99,
    "category": "Electronics"
  }'
```

3. **Get All Products**:

```bash
curl -X GET "http://localhost:5105/api/products"
```

4. **Create an Order**:

```bash
curl -X POST "http://localhost:5105/api/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "customerEmail": "john.doe@example.com",
    "shippingAddress": "123 Main St, Anytown, USA",
    "items": [
      {
        "productSku": "LAPTOP-001",
        "quantity": 1
      }
    ]
  }'
```

5. **Get All Orders**:

```bash
curl -X GET "http://localhost:5105/api/orders"
```

### Verify Binary File Storage

After running the above commands, check the `Data` directory in the project root:

- `Data/products.json` - Should contain the product data
- `Data/orders.json` - Should contain the order data
- `Data/inventory-items.json` - Should contain inventory data (if any)

### Test Data Persistence

1. Stop the API (Ctrl+C)
2. Restart the API
3. Make GET requests to verify data persists across restarts

This demonstrates that the repositories now store data in JSON files on disk instead of just in memory.
