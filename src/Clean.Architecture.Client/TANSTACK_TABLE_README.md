# TanStack Table Implementation

This project now includes a powerful and reusable TanStack Table implementation with the following components:

## Components Added

### 1. `DataTable` Component (`src/components/ui/data-table.tsx`)

A reusable, feature-rich table component with:

- **Sorting**: Click column headers to sort data
- **Global Search**: Filter across all columns with search input
- **Pagination**: Navigate through large datasets with customizable page sizes
- **Responsive Design**: Works well on mobile and desktop

### 2. `ProductManagementTable` Component

Replaces the card view with a clean table interface for product management:

- Displays all product information in a sortable table
- Includes status badges, price formatting, and action menus
- Supports product creation via dialog
- Category filtering with dropdown

### 3. `InventoryManagementTable` Component

Table view for inventory management:

- Visual stock level indicators with progress bars
- Color-coded status badges (Out of Stock, Low Stock, In Stock)
- Stock adjustment functionality via action menu
- Real-time inventory updates

### 4. `TanStackTableExample` Component

Demonstrates advanced table features:

- Custom cell rendering with badges and formatting
- Multiple data types (text, numbers, dates, status)
- Action buttons in cells
- Statistics cards above the table

## Key Features

### Sorting

- Click any column header to sort
- Visual indicators show sort direction (ascending/descending)
- Supports multiple data types (text, numbers, dates)

### Filtering

- Global search across all columns
- Real-time filtering as you type
- Category/status filters for specific views

### Pagination

- Configurable page sizes (10, 20, 30, 40, 50)
- First/Previous/Next/Last navigation
- Shows current page and total pages

### Custom Cell Rendering

- Status badges with color coding
- Formatted currency display
- Date/time formatting
- Progress bars for stock levels
- Action buttons and dropdown menus

## Usage Example

```tsx
import { DataTable } from "@/components/ui/data-table";
import { createSortableHeader } from "@/lib/table-utils";
import type { ColumnDef } from "@tanstack/react-table";

// Define your data type
interface User {
  id: string;
  name: string;
  email: string;
  status: "active" | "inactive";
}

// Define columns
const columns: ColumnDef<User>[] = [
  {
    accessorKey: "name",
    header: createSortableHeader<User>("Name"),
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue("name")}</div>
    ),
  },
  {
    accessorKey: "status",
    header: createSortableHeader<User>("Status"),
    cell: ({ row }) => {
      const status = row.getValue("status") as string;
      return (
        <span
          className={`px-2 py-1 rounded text-xs ${
            status === "active"
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {status}
        </span>
      );
    },
  },
];

// Use in component
function UserTable({ users }: { users: User[] }) {
  return (
    <DataTable
      columns={columns}
      data={users}
      searchPlaceholder="Search users..."
      enablePagination={true}
      enableSorting={true}
      enableFiltering={true}
      pageSize={10}
    />
  );
}
```

## Navigation

The application now includes:

- **Products Page**: Uses `ProductManagementTable` by default
- **Inventory Page**: Can be updated to use `InventoryManagementTable`
- **Table Example**: Access via component import for demonstrations

## Styling

The tables use Tailwind CSS classes and are fully responsive. They integrate seamlessly with your existing UI components:

- Radix UI primitives for accessibility
- Consistent with your design system
- Dark mode compatible (if implemented)

## Dependencies Added

- `@tanstack/react-table`: Core table functionality
- All existing Radix UI components remain compatible

The implementation provides a solid foundation for data tables throughout your Clean Architecture application while maintaining type safety and performance.
