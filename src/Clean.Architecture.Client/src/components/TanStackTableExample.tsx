import React, { useMemo } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { DataTable } from "@/components/ui/data-table";
import { createSortableHeader } from "@/lib/table-utils";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

// Sample data for demonstration
interface SampleData {
  id: string;
  name: string;
  email: string;
  status: "active" | "inactive" | "pending";
  role: string;
  department: string;
  salary: number;
  joinDate: string;
  lastLogin: string;
}

const sampleData: SampleData[] = [
  {
    id: "1",
    name: "John Doe",
    email: "john.doe@example.com",
    status: "active",
    role: "Software Engineer",
    department: "Engineering",
    salary: 95000,
    joinDate: "2022-01-15",
    lastLogin: "2025-09-11T10:30:00Z",
  },
  {
    id: "2",
    name: "Jane Smith",
    email: "jane.smith@example.com",
    status: "active",
    role: "Product Manager",
    department: "Product",
    salary: 110000,
    joinDate: "2021-11-03",
    lastLogin: "2025-09-11T09:15:00Z",
  },
  {
    id: "3",
    name: "Bob Johnson",
    email: "bob.johnson@example.com",
    status: "inactive",
    role: "Designer",
    department: "Design",
    salary: 80000,
    joinDate: "2023-03-20",
    lastLogin: "2025-09-10T16:45:00Z",
  },
  {
    id: "4",
    name: "Alice Wilson",
    email: "alice.wilson@example.com",
    status: "pending",
    role: "Data Analyst",
    department: "Analytics",
    salary: 75000,
    joinDate: "2025-09-01",
    lastLogin: "2025-09-11T08:00:00Z",
  },
  {
    id: "5",
    name: "Charlie Brown",
    email: "charlie.brown@example.com",
    status: "active",
    role: "DevOps Engineer",
    department: "Engineering",
    salary: 105000,
    joinDate: "2022-07-12",
    lastLogin: "2025-09-11T11:20:00Z",
  },
];

const TanStackTableExample: React.FC = () => {
  // Define table columns with various features
  const columns: ColumnDef<SampleData>[] = useMemo(
    () => [
      {
        accessorKey: "name",
        header: createSortableHeader<SampleData>("Name"),
        cell: ({ row }) => (
          <div className="font-medium">{row.getValue("name")}</div>
        ),
      },
      {
        accessorKey: "email",
        header: createSortableHeader<SampleData>("Email"),
        cell: ({ row }) => (
          <div className="lowercase">{row.getValue("email")}</div>
        ),
      },
      {
        accessorKey: "status",
        header: createSortableHeader<SampleData>("Status"),
        cell: ({ row }) => {
          const status = row.getValue("status") as string;
          const variants = {
            active: "bg-green-100 text-green-800",
            inactive: "bg-red-100 text-red-800",
            pending: "bg-yellow-100 text-yellow-800",
          };

          return (
            <span
              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                variants[status as keyof typeof variants]
              }`}
            >
              {status}
            </span>
          );
        },
      },
      {
        accessorKey: "role",
        header: createSortableHeader<SampleData>("Role"),
        cell: ({ row }) => (
          <div className="font-medium">{row.getValue("role")}</div>
        ),
      },
      {
        accessorKey: "department",
        header: createSortableHeader<SampleData>("Department"),
        cell: ({ row }) => (
          <Badge variant="secondary">{row.getValue("department")}</Badge>
        ),
      },
      {
        accessorKey: "salary",
        header: createSortableHeader<SampleData>("Salary"),
        cell: ({ row }) => {
          const salary = parseFloat(row.getValue("salary"));
          return (
            <div className="text-right font-medium">
              ${salary.toLocaleString()}
            </div>
          );
        },
      },
      {
        accessorKey: "joinDate",
        header: createSortableHeader<SampleData>("Join Date"),
        cell: ({ row }) => {
          const date = new Date(row.getValue("joinDate"));
          return <div className="text-sm">{date.toLocaleDateString()}</div>;
        },
      },
      {
        accessorKey: "lastLogin",
        header: createSortableHeader<SampleData>("Last Login"),
        cell: ({ row }) => {
          const date = new Date(row.getValue("lastLogin"));
          return (
            <div className="text-sm text-muted-foreground">
              {date.toLocaleString()}
            </div>
          );
        },
      },
      {
        id: "actions",
        header: "Actions",
        cell: ({ row }) => {
          const user = row.original;

          return (
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => console.log("Edit user:", user.id)}
              >
                Edit
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => console.log("View user:", user.id)}
              >
                View
              </Button>
            </div>
          );
        },
      },
    ],
    []
  );

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            TanStack Table Example
          </h1>
          <p className="text-muted-foreground">
            Demonstration of TanStack Table features with sorting, filtering,
            and pagination
          </p>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Users</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{sampleData.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Users</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {sampleData.filter((u) => u.status === "active").length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending Users</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-yellow-600">
              {sampleData.filter((u) => u.status === "pending").length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Avg. Salary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              $
              {Math.round(
                sampleData.reduce((sum, u) => sum + u.salary, 0) /
                  sampleData.length
              ).toLocaleString()}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Table */}
      <Card>
        <CardHeader>
          <CardTitle>User Management</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={sampleData}
            searchPlaceholder="Search users by name, email, or department..."
            enablePagination={true}
            enableSorting={true}
            enableFiltering={true}
            pageSize={5}
          />
        </CardContent>
      </Card>

      {/* Features List */}
      <Card>
        <CardHeader>
          <CardTitle>TanStack Table Features Demonstrated</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <h4 className="font-semibold mb-2">Core Features</h4>
              <ul className="space-y-1 text-sm text-muted-foreground">
                <li>✅ Column sorting (click headers)</li>
                <li>✅ Global search filtering</li>
                <li>✅ Pagination with page size controls</li>
                <li>✅ Custom cell rendering</li>
                <li>✅ Responsive design</li>
              </ul>
            </div>
            <div>
              <h4 className="font-semibold mb-2">UI Components</h4>
              <ul className="space-y-1 text-sm text-muted-foreground">
                <li>✅ Status badges with colors</li>
                <li>✅ Formatted currency display</li>
                <li>✅ Date formatting</li>
                <li>✅ Action buttons</li>
                <li>✅ Sortable headers with icons</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default TanStackTableExample;
